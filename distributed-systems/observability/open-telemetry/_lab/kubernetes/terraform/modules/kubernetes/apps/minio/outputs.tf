output "main_tenant_endpoint" {
  value = "main-hl.${var.namespace}.svc.cluster.local:9000"
}

output "main_tenant_access_key" {
  value = random_string.main_tenant_access_key.result
}

output "main_tenant_secret_access" {
  value = random_string.main_tenant_secret_key.result
}
