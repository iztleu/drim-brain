variable "namespace" {
  type = string
  default = "observability"
}

variable "storage_class_name" {
  type = string
}

variable "grafana_storage_size" {
  type = string
}

variable "grafana_url" {
  type = string
}

variable "cert_manager_cluster_issuer" {
  type = string
}

variable "traefik_entry_point" {
  type = string
}
