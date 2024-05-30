variable "namespace" {
  type = string
  default = "observability"
}

variable "storage_class_name" {
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

variable "prometheus_storage_size" {
  type = string
}

variable "prometheus_url" {
  type = string
}

variable "alertmanager_url" {
  type = string
}

variable "s3_endpoint" {
  type = string
}

variable "s3_access_key_id" {
  type = string
}

variable "s3_secret_access_key" {
  type = string
}

variable "loki_chunks_cache_allocated_memory" {
  type = number
  default = 4096
}
