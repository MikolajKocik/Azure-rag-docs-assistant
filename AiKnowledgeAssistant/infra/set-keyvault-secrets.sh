#!/usr/bin/env bash
set -euo pipefail

KEY_VAULT_NAME="$(terraform output -raw key_vault_name)"

SEARCH_ENDPOINT="$(terraform output -raw azure_search_endpoint)"
SEARCH_KEY="$(terraform output -raw azure_search_key)"

OPENAI_ENDPOINT="$(terraform output -raw azure_openai_endpoint)"
OPENAI_KEY="$(terraform output -raw azure_openai_key)"

STORAGE_CONNECTION_STRING="$(terraform output -raw storage_connection_string)"

BLOB_CONTAINER_NAME="$(terraform output -raw blob_container_name)"
BLOB_EXTRACT_CONTAINER_NAME="$(terraform output -raw blob_extract_container_name)"
BLOB_COPY_CONTAINER_NAME="$(terraform output -raw blob_copy_container_name)"

DOCUMENT_INTELLIGENCE_ENDPOINT="$(terraform output -raw document_intelligence_endpoint)"
DOCUMENT_INTELLIGENCE_KEY="$(terraform output -raw document_intelligence_key)"

echo "Setting secrets in Key Vault: ${KEY_VAULT_NAME}"

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--search-endpoint" \
  --value "$SEARCH_ENDPOINT" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--search-key" \
  --value "$SEARCH_KEY" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--OpenAI--Endpoint" \
  --value "$OPENAI_ENDPOINT" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--OpenAI--Key" \
  --value "$OPENAI_KEY" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--StorageConnectionString" \
  --value "$STORAGE_CONNECTION_STRING" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--BlobContainerName" \
  --value "$BLOB_CONTAINER_NAME" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--FunctionAppBlobContainerNameExtract" \
  --value "$BLOB_EXTRACT_CONTAINER_NAME" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--FunctionAppBlobContainerNameCopy" \
  --value "$BLOB_COPY_CONTAINER_NAME" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--Form-Recognizer-Endpoint" \
  --value "$DOCUMENT_INTELLIGENCE_ENDPOINT" \
  --only-show-errors >/dev/null

az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "Azure--Form-Recognizer-Key" \
  --value "$DOCUMENT_INTELLIGENCE_KEY" \
  --only-show-errors >/dev/null

echo "Done."
echo "Set this environment variable for local app:"
terraform output -raw local_environment_variable
