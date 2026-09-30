# AI notes 03: embeddings and cosine similarity

Model: `all-minilm` (local, Ollama). Code: `labs/Lab01Embeddings`.

- **Embedding:** a list of numbers (384 for all-minilm) that represents the meaning of a text. Texts with similar meaning get similar lists.
- **Cosine similarity:** measures how close two embeddings point in the same direction. 1.0 means the same meaning, around 0 means unrelated. I calculated it myself in plain C#: dot product ÷ (length of a × length of b), with no AI library.
- **The experiment:** I compared "Pump P-101 is leaking oil" with a same-meaning sentence, an unrelated equipment problem and a leave request. A sentence compared with itself scores 1.000; the closer the meaning, the higher the score. Run `dotnet run --project labs/Lab01Embeddings` with Ollama running to see the scores.

This is the basis of semantic search (Lab02) and of retrieval in RAG.