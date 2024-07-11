variable "namespace" {
  type = string
  default = "argocd"
}

variable "url" {
  type = string
}

variable "traefik_entry_point" {
  type = string
}

variable "cert_manager_cluster_issuer" {
  type = string
}
