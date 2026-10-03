namespace FieldOps.Api.Rag;

// Turns text into a vector with Ollama's /api/embed: the same call you made in Lab02 and Lab03
public class OllamaEmbedder(HttpClient http, IConfiguration config)
{
    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var request = new { model = config["Ai:EmbeddingModel"], input = text };
        var response = await http.PostAsJsonAsync("/api/embed", request, ct);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(ct);
        return body!.Embeddings[0];
    }
}

// Ollama replies { "embeddings": [[0.01, -0.2, ...]] }: one vector per input
public record OllamaEmbedResponse(float[][] Embeddings);