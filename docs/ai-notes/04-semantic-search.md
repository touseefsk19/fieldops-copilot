# AI notes 04: semantic search over maintenance requests

Model: `all-minilm` (local, Ollama, 384 numbers per text). Code: `labs/Lab02SemanticSearch`.

## The 3 phases
1. **Index:** turn every maintenance request into a vector once, at the start, and keep the text and vector together in a list.
2. **Query:** turn the user's question into a vector with the same model.
3. **Rank:** compare the question vector with every request using cosine similarity, sort highest first, and show the top 3.

Documents are embedded once; only the question is embedded on each search. A real system keeps the vectors in a vector store (Azure AI Search, pgvector) instead of a List.

