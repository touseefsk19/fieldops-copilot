# AI notes 05: RAG by hand in C#

Models: `all-minilm` (embeddings) + `qwen2.5:0.5b` (chat), both local via Ollama. Code: `labs/Lab03Rag`. No AI library: plain HttpClient and my own cosine similarity.

## The 5 steps
1. **Index:** embed each document once and keep text + vector together.
2. **Retrieve:** embed the question and take the top 2 documents by cosine similarity.
3. **Gate:** if the best score is below 0.35, refuse ("I don't have a procedure for that"). No evidence = no answer, and no LLM call is paid for.
4. **Augment:** put the retrieved documents into the prompt, numbered [1] and [2].
5. **Generate:** the model must answer only from those sources, cite [1]/[2], or say "I don't know".

## What the gate prevents
Hallucination at the source: an off-topic question (e.g. baking a cake) never reaches the model, so it can't make up an answer.

## What can still go wrong
Even when retrieval finds the right document, a small model can answer wrongly or skip the citation. That's a generation failure, not a retrieval one. It's caught by citation verification (checking that each claim is supported by the cited source) and by evals with a golden question set (AI bonus 08).

## Why it matters
This is the core of every enterprise RAG system, and of my Meridian project: retrieve → gate → augment → generate with citations. In production the List becomes a vector store (Azure AI Search) and the HTTP call becomes IChatClient.