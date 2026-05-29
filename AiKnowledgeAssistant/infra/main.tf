data "azurerm_client_config" "current" {}

resource "random_string" "suffix" {
  length  = 6
  upper   = false
  special = false
}

locals {
  name_prefix = lower("${var.project_name}-${var.environment}-${random_string.suffix.result}")

  blob_primary_container = "documents"
  blob_extract_container = "extracted-metadata"
  blob_copy_container    = "document-copies"

  search_index_name = "documents-index"
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${local.name_prefix}"
  location = var.location
  tags     = var.tags
}

resource "azurerm_key_vault" "main" {
  name                        = "kv-${local.name_prefix}"
  location                    = azurerm_resource_group.main.location
  resource_group_name         = azurerm_resource_group.main.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  enable_rbac_authorization   = true
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  tags                        = var.tags
}

resource "azurerm_role_assignment" "current_user_key_vault_secrets_officer" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_storage_account" "main" {
  name                     = replace("st${var.project_name}${var.environment}${random_string.suffix.result}", "-", "")
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = var.storage_replication_type
  min_tls_version          = "TLS1_2"
  tags                     = var.tags
}

resource "azurerm_storage_container" "documents" {
  name                  = local.blob_primary_container
  storage_account_id    = azurerm_storage_account.main.id
  container_access_type = "private"
}

resource "azurerm_storage_container" "extracted_metadata" {
  name                  = local.blob_extract_container
  storage_account_id    = azurerm_storage_account.main.id
  container_access_type = "private"
}

resource "azurerm_storage_container" "document_copies" {
  name                  = local.blob_copy_container
  storage_account_id    = azurerm_storage_account.main.id
  container_access_type = "private"
}

resource "azurerm_search_service" "main" {
  name                = "srch-${local.name_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = var.search_sku
  replica_count       = 1
  partition_count     = 1
  tags                = var.tags
}

resource "azurerm_cognitive_account" "openai" {
  name                          = "oai-${local.name_prefix}"
  location                      = azurerm_resource_group.main.location
  resource_group_name           = azurerm_resource_group.main.name
  kind                          = "OpenAI"
  sku_name                      = var.openai_sku
  custom_subdomain_name         = "oai-${local.name_prefix}"
  public_network_access_enabled = true
  tags                          = var.tags
}

resource "azurerm_cognitive_account" "document_intelligence" {
  name                = "di-${local.name_prefix}"
  location            = var.document_intelligence_location
  resource_group_name = azurerm_resource_group.main.name

  kind     = "FormRecognizer"
  sku_name = "S0"

  public_network_access_enabled = true

  tags = var.tags
}
