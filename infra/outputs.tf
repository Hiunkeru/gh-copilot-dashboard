output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "app_service_url" {
  value = module.app_service.app_url
}

output "static_web_app_url" {
  value = module.static_web_app.default_hostname
}

output "static_web_app_api_key" {
  value     = module.static_web_app.api_key
  sensitive = true
}

output "sql_server_fqdn" {
  value = module.sql.server_fqdn
}

output "keyvault_uri" {
  value = module.keyvault.vault_uri
}
