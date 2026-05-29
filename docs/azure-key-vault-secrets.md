# Azure Key Vault Secrets

## App configuration

| Name | Source |
|---|---|
| `KEYVAULT_URI` | Environment / app configuration |

## Azure AI Search

| Secret name |
|---|
| `Azure--search-endpoint` |
| `Azure--search-key` |

## Azure OpenAI

| Secret name |
|---|
| `Azure--OpenAI--Endpoint` |
| `Azure--OpenAI--Key` |

## Azure Blob Storage

| Secret name |
|---|
| `Azure--StorageConnectionString` |
| `Azure--BlobContainerName` |
| `Azure--FunctionAppBlobContainerNameExtract` |
| `Azure--FunctionAppBlobContainerNameCopy` |

## Non-secret configuration

| Name | Default |
|---|---|
| `AzureSearch:IndexName` | `documents-index` |
| `Rag:Ranker` | - |
| `Ranker:ModelPath` | - |
| `Ranker:TokenizerPath` | - |
| `Rag:VectorTopK` | `50` |
| `Rag:FinalTopK` | `3` |
| `AzureOpenAI:ChatDeploymentName` | - |
| `AzureOpenAI:EmbeddingDeploymentName` | - |
| `Ingestion:DocumentAnalysisModelId` | `prebuilt-document` |
| `Ingestion:MaxTokensPerLine` | `100` |
| `Ingestion:MaxTokensPerParagraph` | `1000` |