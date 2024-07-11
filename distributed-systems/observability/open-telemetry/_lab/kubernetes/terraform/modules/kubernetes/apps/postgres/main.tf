locals {
    namespace = kubernetes_namespace.postgres.metadata[0].name
}

resource "kubernetes_namespace" "postgres" {
  metadata {
    name = var.namespace
  }
}

resource "helm_release" "postgres" {
  name = "postgres"
  repository = "oci://registry-1.docker.io/bitnamicharts"
  chart = "postgresql"
  version = "14.2.3"

  namespace = local.namespace

  values = [
    templatefile("${path.module}/helm/values.yaml.tpl", {
      username = var.username,
      password = var.password,
      postgres_password = var.postgres_password,
      storage_class_name = var.storage_class_name,
      storage_size = var.storage_size
    })
  ]
}
