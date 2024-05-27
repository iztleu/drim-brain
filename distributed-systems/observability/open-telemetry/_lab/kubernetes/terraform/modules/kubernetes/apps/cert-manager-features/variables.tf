variable "acme_email" {
  type = string
}

variable "cluster_issuer" {
    type = string
    default = "letsencrypt"
}
