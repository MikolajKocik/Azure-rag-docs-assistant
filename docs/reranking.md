# Local ONNX Reranker

## Purpose

The reranker improves RAG context selection after vector search.

Vector search retrieves candidate chunks based on embedding similarity. This is fast, but the top results may be semantically related without directly answering the user question.

The reranker scores each query-chunk pair and sorts the candidates by relevance before passing the final chunks to the LLM.

## Pipeline

```text
User question
→ Question embedding
→ Azure AI Search vector retrieval
→ Candidate chunks
→ Local ONNX reranker
→ Top ranked chunks
→ LLM answer generation
```

## How it works

The local reranker uses an ONNX cross-encoder model.
Unlike embedding similarity, a cross-encoder receives both the query and the document chunk as one input pair:

```text
<s> query </s></s> document chunk </s>
```

The model returns a raw score/logit for the pair. The application applies a sigmoid function to convert it into a score-like value:

```text
score = sigmoid(logit)
```

Chunks are then sorted by score in descending order.

## Baseline

The baseline strategy is `NoOpChunkRanker`.
It keeps the original vector search order and does not apply model-based reranking.
This allows comparing:

```text
Vector search only
vs
Vector search + ONNX reranking
```

## Experiment setup

Typical comparison:

```text
A: vector search top 3 → LLM
B: vector search top 50 → reranker → top 3 → LLM
```

Useful metrics:

- retrieval hit rate
- answer relevance
- faithfulness
- latency
- token usage
- estimated cost

## Trade-offs

Benefits:

- better context precision
- fewer irrelevant chunks in the final prompt
- potentially higher answer quality

Costs:

- additional latency
- more CPU usage
- more complex pipeline
- model/tokenizer maintenance

## Engineering note

The reranker is implemented behind the `IChunkRanker` interface.
This allows switching between:

- `NoOpChunkRanker`
- `LocalRankerService`

without changing the main RAG pipeline logic.
The selected ranker can be controlled through configuration, which makes benchmarking and experimentation easier.