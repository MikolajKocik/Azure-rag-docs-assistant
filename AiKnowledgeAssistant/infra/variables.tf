variable "project_name" {
  description = "Short project name used in Azure resource names."
  type        = string
  default     = "aika"
}

variable "environment" {
  description = "Environment name."
  type        = string
  default     = "dev"
}

variable "location" {
  description = "Azure region. Change this if Azure OpenAI is not available in the default region for your subscription."
  type        = string
  default     = "polandcentral"
}

variable "search_sku" {
  description = "Azure AI Search SKU."
  type        = string
  default     = "basic"
}

variable "openai_sku" {
  description = "Azure OpenAI SKU."
  type        = string
  default     = "S0"
}

variable "storage_replication_type" {
  description = "Storage replication type."
  type        = string
  default     = "LRS"
}

variable "tags" {
  description = "Common Azure tags."
  type        = map(string)
  default = {
    project = "ai-knowledge-assistant"
  }
}

variable "document_intelligence_location" {
  description = "Azure region for Document Intelligence / Form Recognizer."
  type        = string
  default     = "swedencentral"
}