output "namespace" {
  value = local.namespace
}

output "username" {
  value = var.username
}

output "password" {
  value = var.password
}

output "postgres_password" {
  value = var.postgres_password
}

output "host" {
    value = "postgres-postgresql.${local.namespace}.svc.cluster.local"
}
