resource "azurerm_service_plan" "main" {
  name                = "asp-${var.project_name}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = var.environment == "prod" ? "B2" : "B1"
  tags                = var.tags
}

resource "azurerm_linux_web_app" "main" {
  name                = "app-${var.project_name}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.main.id
  tags                = var.tags

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = var.dotnet_version
    }
    always_on = var.environment == "prod"
  }

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.app_insights_connection_string
    "APPINSIGHTS_INSTRUMENTATIONKEY"        = var.app_insights_instrumentation_key
    "KeyVault__Uri"                         = var.keyvault_uri
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = var.sql_connection_string
  }

  connection_string {
    name  = "StorageAccount"
    type  = "Custom"
    value = var.storage_connection_string
  }
}

# Grant the App Service managed identity access to Key Vault secrets
resource "azurerm_key_vault_access_policy" "app_service" {
  key_vault_id = var.keyvault_id
  tenant_id    = azurerm_linux_web_app.main.identity[0].tenant_id
  object_id    = azurerm_linux_web_app.main.identity[0].principal_id

  secret_permissions = [
    "Get", "List"
  ]
}
