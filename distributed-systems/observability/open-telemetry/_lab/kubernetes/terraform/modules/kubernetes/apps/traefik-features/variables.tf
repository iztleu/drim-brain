variable "namespace" {
  type = string
  default = "traefik"
}

variable "traefik_entry_point" {
  type = string
}

variable "dashboard_url" {
  type = string
}

variable "cert_manager_cluster_issuer" {
  type = string
}
