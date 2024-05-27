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
}

module "observability" {
  source = "../../modules/kubernetes/apps/observability"

  storage_class_name = "hcloud-volumes"
  grafana_storage_size = var.grafana_storage_size
  grafana_url = "grafana.${var.infra_url}"
  cert_manager_cluster_issuer = module.cert_manager_features.cluster_issuer
  traefik_entry_point = var.traefik_entry_point
}
