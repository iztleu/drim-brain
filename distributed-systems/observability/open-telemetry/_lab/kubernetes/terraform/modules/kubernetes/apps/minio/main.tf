locals {
  minio_console_cert = "minio-console-cert"
}

resource "kubernetes_namespace" "minio" {
  metadata {
    name = var.namespace
  }
}

resource "helm_release" "minio-operator" {
  name       = "minio-operator"
  repository = "https://operator.min.io"
  chart      = "operator"
  version    = "5.0.15"
  namespace  = kubernetes_namespace.minio.metadata[0].name
}

resource "kubernetes_manifest" "minio_console_cert" {
  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind       = "Certificate"
    metadata = {
      name      = local.minio_console_cert
      namespace = kubernetes_namespace.minio.metadata[0].name
    }
    spec = {
      commonName = var.minio_console_url
      secretName = local.minio_console_cert
      dnsNames = [
        var.minio_console_url
      ]
      issuerRef = {
        kind = "ClusterIssuer"
        name = var.cert_manager_cluster_issuer
      }
    }
  }
}

resource "kubernetes_manifest" "minio_console_ingressroute" {
  manifest = {
    apiVersion = "traefik.io/v1alpha1"
    kind       = "IngressRoute"
    metadata = {
      name      = "minio-console"
      namespace = kubernetes_namespace.minio.metadata[0].name
    }
    spec = {
      entryPoints = [
        var.traefik_entry_point
      ]
      routes = [
        {
          match = "Host(`${var.minio_console_url}`)"
          kind  = "Rule"
          services = [
            {
              name = "console"
              port = 9090
            }
          ]
        }
      ]
      tls = {
        secretName = local.minio_console_cert
      }
    }
  }
}

resource "random_string" "main_tenant_access_key" {
  length = 16
  special = true
}

resource "random_string" "main_tenant_secret_key" {
  length = 16
  special = true
}

resource "kubernetes_secret" "main_tenant_keys" {
  metadata {
    name = "main-tenant-keys"
    namespace = kubernetes_namespace.minio.metadata[0].name
  }
  data = {
    accessKey = random_string.main_tenant_access_key.result
    secretKey = random_string.main_tenant_secret_key.result
  }
}

resource "helm_release" "minio-tenant" {
  name       = "minio-tenant"
  repository = "https://operator.min.io"
  chart      = "tenant"
  version    = "5.0.15"
  namespace  = kubernetes_namespace.minio.metadata[0].name
  values = [
    templatefile("${path.module}/helm/main-tenant/values.yaml.tpl", {
      storage_class_name = var.storage_class_name,
      storage_size = var.main_tenant_storage_size,
      access_key = random_string.main_tenant_access_key.result,
      secret_key = random_string.main_tenant_secret_key.result,
    })
  ]
}
