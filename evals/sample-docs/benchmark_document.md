# AI Knowledge Assistant — System Specification

## Overview

The AI Knowledge Assistant is a document-grounded question-answering system. Users upload documents, which are processed, chunked, and indexed. At query time, the system retrieves relevant chunks and uses a language model to generate answers strictly grounded in the retrieved context.

---

## Document Ingestion

### Upload Constraints

- Users may upload documents up to **30 MB** in size.
- Uploaded files are stored in **Azure Blob Storage**.

### Storage Layout

| Container | Purpose |
|---|---|
| Primary blob container | Stores original uploaded files |
| Extract container | Stores extracted metadata produced during processing |
| Copy container | Stores copied versions of uploaded files |

### Text Extraction

- Text is extracted using **Azure Document Intelligence** with the **`prebuilt-document`** model.
- The extraction pipeline handles PDFs, Office documents, and image-based files.

### Chunking

Extracted text is split into chunks according to the following rules:

- Maximum **100 tokens per line**
- Maximum **1000 tokens per paragraph**

Chunking preserves semantic boundaries where possible to improve retrieval quality.

### Embedding

- Each chunk is embedded using the configured **Azure OpenAI embedding deployment**.
- Chunks and their corresponding embedding vectors are stored in **Azure AI Search**.
- The search index name is **`documents-index`**.

---

## Query & Retrieval

### Query Embedding

At query time, the application generates an embedding vector for the user's question using the same Azure OpenAI embedding deployment used during ingestion.

### Retrieval Strategies

The system supports two retrieval strategies, selectable through configuration.

#### Baseline — Vector Retrieval Only

- Performs a vector similarity search against `documents-index`.
- Returns the **top 3** results directly as the final context.
- Implemented by **`NoOpChunkRanker`**, which passes candidates through without reranking.

#### Reranking — Vector Retrieval + Local ONNX Cross-Encoder

- Performs a vector similarity search and retrieves the **top 50** candidate chunks.
- Passes the 50 candidates through a **local ONNX cross-encoder reranker**.
- The reranker scores each (question, chunk) pair and selects the **final top 3** chunks.
- Implemented by **`LocalRankerService`**.

### Ranker Interface & Configuration

Both ranker implementations satisfy the **`IChunkRanker`** interface, which decouples retrieval logic from the rest of the pipeline.

The active ranker is selected through application configuration:

```
Rag:Ranker
```

Set this key to the desired ranker implementation name (e.g., `NoOpChunkRanker` or `LocalRankerService`).

---

## Answer Generation

- The language model receives only the **retrieved chunks** as context.
- The assistant is instructed to answer **exclusively from the provided context**.
- If the answer cannot be found in the retrieved context, the assistant responds that it **does not have information about the question**.

---

## Component Summary

| Component | Technology / Value |
|---|---|
| File storage | Azure Blob Storage |
| Text extraction | Azure Document Intelligence (`prebuilt-document`) |
| Embedding model | Azure OpenAI embedding deployment |
| Search index | Azure AI Search — `documents-index` |
| Max upload size | 30 MB |
| Chunk size (line) | 100 tokens |
| Chunk size (paragraph) | 1000 tokens |
| Baseline retrieval | Top 3 vector results (`NoOpChunkRanker`) |
| Reranking retrieval | Top 50 → reranked → Top 3 (`LocalRankerService`) |
| Ranker interface | `IChunkRanker` |
| Ranker config key | `Rag:Ranker` |
