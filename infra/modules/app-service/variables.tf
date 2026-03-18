variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "environment" { type = string }
variable "project_name" { type = string }
variable "dotnet_version" { type = string }
variable "sql_connection_string" { type = string; sensitive = true }
variable "keyvault_uri" { type = string }
variable "keyvault_id" { type = string }
variable "app_insights_connection_string" { type = string }
variable "app_insights_instrumentation_key" { type = string }
variable "tags" { type = map(string) }
