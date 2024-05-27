resource "kubernetes_namespace" "observability" {
  metadata {
    name = var.namespace
  }
}

# Grafana

locals {
  grafana_cert = "grafana-cert"
}

resource "random_string" "grafana_admin_password" {
  length = 16
  special = true
}

resource "kubernetes_secret" "grafana_admin_credentials" {
  metadata {
    name = "grafana-admin-credentials"
    namespace = kubernetes_namespace.observability.metadata[0].name
  }
  data = {
    admin-user = "admin"
    admin-password = random_string.grafana_admin_password.result
  }
}

resource "helm_release" "grafana" {
  name       = "grafana"
  repository = "https://grafana.github.io/helm-charts"
  chart      = "grafana"
  version    = "7.3.11"
  namespace  = kubernetes_namespace.observability.metadata[0].name

  values = [
    templatefile("${path.module}/grafana/values.yaml", {
      storage_class_name = var.storage_class_name,
      storage_size = var.grafana_storage_size,
      admin_secret_name = kubernetes_secret.grafana_admin_credentials.metadata[0].name,
    })
  ]
}

resource "kubernetes_manifest" "grafana_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind       = "Certificate"
    metadata = {
      name      = local.grafana_cert
      namespace = kubernetes_namespace.observability.metadata[0].name
    }
    spec = {
      commonName = var.grafana_url
      secretName = local.grafana_cert
      dnsNames = [
        var.grafana_url
      ]
      issuerRef = {
        kind = "ClusterIssuer"
        name = var.cert_manager_cluster_issuer
      }
    }
  }
}

resource "kubernetes_manifest" "grafana_ingressroute" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind       = "IngressRoute"
    metadata = {
      name      = "ui"
      namespace = var.namespace
    }
    spec = {
      entryPoints = [
        var.traefik_entry_point
      ]
      routes = [
        {
          match = "Host(`${var.grafana_url}`)"
          kind  = "Rule"
          services = [
            {
              name = "grafana"
              port = 80
            }
          ]
        }
      ]
      tls = {
        secretName = local.grafana_cert
      }
    }
  }
}
