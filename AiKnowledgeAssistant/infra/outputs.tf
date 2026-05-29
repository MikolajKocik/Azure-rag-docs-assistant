output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "key_vault_name" {
  value = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "azure_search_endpoint" {
  value = "https://${azurerm_search_service.main.name}.search.windows.net"
}

output "azure_search_key" {
  value     = azurerm_search_service.main.primary_key
  sensitive = true
}

output "azure_search_index_name" {
  value = local.search_index_name
}

output "azure_openai_endpoint" {
  value = azurerm_cognitive_account.openai.endpoint
}

output "azure_openai_key" {
  value     = azurerm_cognitive_account.openai.primary_access_key
  sensitive = true
}

output "storage_connection_string" {
  value     = azurerm_storage_account.main.primary_connection_string
  sensitive = true
}

output "blob_container_name" {
  value = azurerm_storage_container.documents.name
}

output "blob_extract_container_name" {
  value = azurerm_storage_container.extracted_metadata.name
}

output "blob_copy_container_name" {
  value = azurerm_storage_container.document_copies.name
}

output "local_environment_variable" {
  value = "KEYVAULT_URI=${azurerm_key_vault.main.vault_uri}"
}

output "document_intelligence_endpoint" {
  value = azurerm_cognitive_account.document_intelligence.endpoint
}

output "document_intelligence_key" {
  value     = azurerm_cognitive_account.document_intelligence.primary_access_key
  sensitive = true
}