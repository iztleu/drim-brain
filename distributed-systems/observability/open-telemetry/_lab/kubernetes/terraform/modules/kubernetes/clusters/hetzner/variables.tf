variable "hcloud_token" {
  sensitive = true
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
