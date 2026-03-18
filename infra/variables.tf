variable "environment" {
  description = "Environment name (dev, prod)"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "West Europe"
}

variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "copilot-dashboard"
}

variable "sql_admin_login" {
  description = "SQL Server admin login"
  type        = string
  default     = "sqladmin"
}

variable "sql_admin_password" {
  description = "SQL Server admin password"
  type        = string
  sensitive   = true
}

variable "dotnet_version" {
  description = ".NET runtime version"
  type        = string
  default     = "v8.0"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    project   = "copilot-dashboard"
    managedBy = "terraform"
  }
}
