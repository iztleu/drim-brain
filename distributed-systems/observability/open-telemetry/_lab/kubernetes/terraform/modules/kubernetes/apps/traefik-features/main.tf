locals {
  dashboard_cert = "dashboard-cert"
}

resource "kubernetes_manifest" "dashboard_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind = "Certificate"
    metadata = {
      name = local.dashboard_cert
      namespace = var.namespace
    }
    spec = {
      commonName = var.dashboard_url
      secretName = local.dashboard_cert
      dnsNames = [
        var.dashboard_url
      ]
      issuerRef = {
        kind = "ClusterIssuer"
        name = var.cert_manager_cluster_issuer
      }
    }
  }
}

resource "kubernetes_manifest" "ingress_route" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind       = "IngressRoute"
    metadata = {
      name      = "dashboard"
      namespace = var.namespace
    }
    spec = {
      routes = [{
        match = "Host(`${var.dashboard_url}`)"
        kind  = "Rule"
        services = [{
          name = "api@internal"
          kind = "TraefikService"
        }]
      }]
      tls = {
        secretName = local.dashboard_cert
      }
    }
  }
}

resource "kubernetes_manifest" "trusted_ips_middleware" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind       = "Middleware"
    metadata = {
      name      = "trusted-ips"
      namespace = var.namespace
    }
    spec = {
      ipAllowList = {
        sourceRange = var.trusted_ips
      }
    }
  }
}
