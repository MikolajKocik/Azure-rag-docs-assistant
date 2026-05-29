# Terraform: AI Knowledge Assistant Infrastructure

Creates the minimum Azure infrastructure for the local RAG app:

- Resource Group
- Key Vault
- Storage Account
- Blob containers
- Azure AI Search
- Azure OpenAI account

Secrets are not created through Terraform resources. Use `set-keyvault-secrets.sh` after `terraform apply`.

## Usage

```bash
az login
az account set --subscription "<SUBSCRIPTION_ID>"

terraform init
terraform plan
terraform apply
```

Then set Key Vault secrets:

```bash
chmod +x set-keyvault-secrets.sh
./set-keyvault-secrets.sh
```

Set local environment variable:

```bash
export KEYVAULT_URI="$(terraform output -raw key_vault_uri)"
```


> [!WARNING]  
> Do not commit:
> - `.terraform/`
> - `terraform.tfstate`
> - `terraform.tfstate.backup`
> - `*.tfvars` with private values


## Model deployments

> [!IMPORTANT]  
> This Terraform creates the Azure OpenAI account, but does not > create model deployments.

Create deployments manually in Azure AI Foundry / Azure Portal, or add Terraform deployment resources later.

App currently expects:

- `AzureOpenAI:ChatDeploymentName = gpt-4-chat`
- `AzureOpenAI:EmbeddingDeploymentName = text-embedding-ada-002`

The deployment names in Azure must match your `appsettings.json`.

## Azure AI Search index

> [!NOTE]  
> This Terraform creates the Azure AI Search service, but not > > the index schema.

Create an index named:

```text
documents-index
```

Minimum fields expected by the app:

- `id`
- `content`
- `embedding`

For `text-embedding-ada-002`, the vector dimension is usually `1536`.
