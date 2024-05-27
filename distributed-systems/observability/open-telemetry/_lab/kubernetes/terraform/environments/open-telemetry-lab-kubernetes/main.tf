module "crypto-bank" {
  source = "../../projects/crypto-bank"

  hcloud_token = var.hcloud_token
  node_ssh_public_key_path = "~/.ssh/open-telemetry-lab-kubernetes.pub"
  node_ssh_private_key_path = "~/.ssh/open-telemetry-lab-kubernetes"
  node_server_type = "cax21"
  node_count = 3
  infra_url = "infra.drim.city"
  acme_email = "dmitriy.melnik@drim.dev"
}
