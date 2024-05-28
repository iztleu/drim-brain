variable "hcloud_token" {
  type = string
  sensitive = true
}

variable "namespace" {
  type = string
  default = "default"
}

variable "node_ssh_public_key_path" {
  type = string
}

variable "node_ssh_private_key_path" {
  type = string
}

variable "node_server_type" {
  type = string
}

variable "node_count" {
  type = number
}

variable "infra_url" {
  type = string
}

variable "traefik_entry_point" {
  type = string
  default = "websecure"
}

variable "acme_email" {
  type = string
}

variable "prometheus_storage_size" {
  type = string
  default = "50Gi"
}

variable "minio_main_tenant_storage_size" {
  type = string
  default = "3Gi"
}
