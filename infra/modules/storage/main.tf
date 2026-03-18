resource "azurerm_storage_account" "main" {
  name                     = replace("st${var.project_name}${var.environment}", "-", "")
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"
  tags                     = var.tags
}

resource "azurerm_storage_table" "reports" {
  name                 = "reports"
  storage_account_name = azurerm_storage_account.main.name
}
