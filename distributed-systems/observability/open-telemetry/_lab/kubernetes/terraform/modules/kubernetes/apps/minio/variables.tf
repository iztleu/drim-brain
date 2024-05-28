variable "namespace" {
  type = string
  default = "minio"
}

variable "storage_class_name" {
  type = string
}

variable "minio_console_url" {
  type = string
}

variable "cert_manager_cluster_issuer" {
  type = string
}

variable "traefik_entry_point" {
  type = string
}

variable "main_tenant_storage_size" {
  type = string
}
