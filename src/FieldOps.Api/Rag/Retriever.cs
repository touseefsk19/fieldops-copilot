using System.Text.Json;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Rag;

public record SearchHit(ManualChunk Chunk, double Score);

// Retrieval in one place, used by /api/ask and /api/eval.
// Registered as Scoped because it uses FieldOpsDbContext, which is Scoped.
public class Retriever(FieldOpsDbContext db, OllamaEmbedder embedder)
{
    public const double MinScore = 0.35; // the gate: below this, a section is not relevant enough

    public async Task<List<SearchHit>> SearchAsync(string question, bool isSupervisor, int topK, CancellationToken ct)
    {
        var questionVector = await embedder.EmbedAsync(question, ct);

        // Access control BEFORE scoring: a Technician's search never loads Supervisor-only sections
        var chunks = await db.ManualChunks
            .AsNoTracking()
            .Where(c => c.Audience == "all" || isSupervisor)
            .ToListAsync(ct);

        return chunks
            .Select(c => new SearchHit(c, VectorMath.Cosine(questionVector, JsonSerializer.Deserialize<float[]>(c.EmbeddingJson)!)))
            .OrderByDescending(h => h.Score)
            .Take(topK)
            .ToList();
    }
}