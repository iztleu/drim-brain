locals {
  storage_class_name = "hcloud-volumes"
}

module "kubernetes_cluster_hetzner" {
  source = "../../modules/kubernetes/clusters/hetzner"

  hcloud_token = var.hcloud_token
  node_ssh_public_key_path = var.node_ssh_public_key_path
  node_ssh_private_key_path = var.node_ssh_private_key_path
  node_server_type = var.node_server_type
  node_count = var.node_count
}

module "cert_manager_features" {
  source = "../../modules/kubernetes/apps/cert-manager-features"

  acme_email = var.acme_email
}

module "traefik_features" {
  source = "../../modules/kubernetes/apps/traefik-features"

  traefik_entry_point = var.traefik_entry_point
  dashboard_url = "traefik.${var.infra_url}"
  cert_manager_cluster_issuer = module.cert_manager_features.cluster_issuer
  trusted_ips = var.trusted_ips
}

module "minio" {
  source = "../../modules/kubernetes/apps/minio"

  storage_class_name = local.storage_class_name
  main_tenant_storage_size = var.minio_main_tenant_storage_size
  minio_console_url = "minio.${var.infra_url}"
  cert_manager_cluster_issuer = module.cert_manager_features.cluster_issuer
  traefik_entry_point = var.traefik_entry_point
}

module "observability" {
  source = "../../modules/kubernetes/apps/observability"

  storage_class_name = local.storage_class_name
  grafana_url = "grafana.${var.infra_url}"
  cert_manager_cluster_issuer = module.cert_manager_features.cluster_issuer
  traefik_entry_point = var.traefik_entry_point
  prometheus_storage_size = var.prometheus_storage_size
  prometheus_url = "prometheus.${var.infra_url}"
  alertmanager_url = "alertmanager.${var.infra_url}"
  s3_endpoint = module.minio.main_tenant_endpoint
  s3_access_key_id = module.minio.main_tenant_access_key
  s3_secret_access_key = module.minio.main_tenant_secret_access
}

module "argocd" {
  source = "../../modules/kubernetes/apps/argocd"

  url = "argocd.${var.infra_url}"
  traefik_entry_point = var.traefik_entry_point
  cert_manager_cluster_issuer = module.cert_manager_features.cluster_issuer
}

resource "random_string" "postgres_password" {
  length = 16
  special = false
}

resource "random_string" "postgres_postgres_password" {
  length = 16
  special = false
}

module "postgres" {
  source = "../../modules/kubernetes/apps/postgres"

  username = "crypto-bank"
  password = random_string.postgres_password.result
  postgres_password = random_string.postgres_postgres_password.result
  storage_class_name = local.storage_class_name
  storage_size = var.postgres_storage_size
}

resource "kubernetes_secret" "postgres-secrets" {
  metadata {
    name = "postgres-secrets"
    namespace = "default"
  }
  data = {
    host = module.postgres.host
    username = module.postgres.username
    password = module.postgres.password
    postgres_password = module.postgres.postgres_password
  }
}
