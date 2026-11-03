using Microsoft.Extensions.AI;

namespace FieldOps.Api.Rag;

// Turns text into a vector through IEmbeddingGenerator, the standard .NET AI abstraction.
// Locally that's Ollama (all-minilm); in Azure it's Azure OpenAI (text-embedding-3-small).
public class Embedder(IEmbeddingGenerator<string, Embedding<float>> generator)
{
    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var vector = await generator.GenerateVectorAsync(text, cancellationToken: ct);
        return vector.ToArray();
    }
}