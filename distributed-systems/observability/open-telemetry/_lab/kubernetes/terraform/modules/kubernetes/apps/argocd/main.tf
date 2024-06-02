locals {
  namespace = kubernetes_namespace.argocd.metadata[0].name
  argocd_cert = "argocd-cert"
}

resource "kubernetes_namespace" "argocd" {
  metadata {
    name = var.namespace
  }
}

resource "helm_release" "argocd" {
  name       = "argocd"
  repository = "https://argoproj.github.io/argo-helm"
  chart      = "argo-cd"
  version    = "6.7.8"

  namespace = local.namespace

  values = [
    templatefile("${path.module}/helm/values.yaml.tpl", {
      url = var.url,
    }),
  ]
}

resource "kubernetes_manifest" "argocd_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind       = "Certificate"
    metadata = {
      name      = local.argocd_cert
      namespace = local.namespace
    }
    spec = {
      commonName = var.url
      secretName = local.argocd_cert
      dnsNames = [
        var.url
      ]
      issuerRef = {
        kind = "ClusterIssuer"
        name = var.cert_manager_cluster_issuer
      }
    }
  }
}

resource "kubernetes_manifest" "argocd_ingressroute" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind       = "IngressRoute"
    metadata = {
      name      = "argocd"
      namespace = local.namespace
    }
    spec = {
      entryPoints = [
        var.traefik_entry_point
      ]
      routes = [
        {
          match = "Host(`${var.url}`)"
          kind  = "Rule"
          services = [
            {
              name = "argocd-server"
              port = 80
            }
          ]
        }
      ]
      tls = {
        secretName = local.argocd_cert
      }
    }
  }
}
