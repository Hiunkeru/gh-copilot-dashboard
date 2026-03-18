output "app_url" {
  value = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "app_name" {
  value = azurerm_linux_web_app.main.name
}

output "principal_id" {
  value = azurerm_linux_web_app.main.identity[0].principal_id
}
