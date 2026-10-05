using System.Text.Json;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Rag;

public enum SearchMode { Vector, Keyword, Hybrid }

// Score = cosine (meaning), KeywordScore = BM25 (exact words), Fused = the hybrid ranking score
public record SearchHit(ManualChunk Chunk, double Score, double KeywordScore = 0, double Fused = 0);

// Retrieval in one place, used by /api/ask and /api/eval.
// Registered as Scoped because it uses FieldOpsDbContext, which is Scoped.
public class Retriever(FieldOpsDbContext db, OllamaEmbedder embedder)
{
    public const double MinScore = 0.35;        // meaning bar
    public const double MinKeywordScore = 2.5;  // keyword bar: a strong match on a rare word
    private const int RrfK = 60;                // standard Reciprocal Rank Fusion constant

    public async Task<List<SearchHit>> SearchAsync(string question, bool isSupervisor, int topK,
        CancellationToken ct, SearchMode mode = SearchMode.Hybrid)
    {
        var questionVector = await embedder.EmbedAsync(question, ct);

        // Access control BEFORE scoring: a Technician's search never loads Supervisor-only sections
        var chunks = await db.ManualChunks
            .AsNoTracking()
            .Where(c => c.Audience == "all" || isSupervisor)
            .ToListAsync(ct);

        var cosine = chunks
            .Select(c => VectorMath.Cosine(questionVector, JsonSerializer.Deserialize<float[]>(c.EmbeddingJson)!))
            .ToArray();
        var keyword = Bm25.Score(question, chunks.Select(c => c.Text).ToList());

        // Rank position in each list (1 = best). Sections with no keyword match get no keyword rank.
        var vectorRank = RankOf(cosine);
        var keywordRank = RankOf(keyword);

        var hits = chunks.Select((c, i) =>
        {
            double fused = 1.0 / (RrfK + vectorRank[i])
                         + (keyword[i] > 0 ? 1.0 / (RrfK + keywordRank[i]) : 0);
            return new SearchHit(c, cosine[i], keyword[i], fused);
        }).ToList();



        if (mode == SearchMode.Hybrid)
        {
            var top = hits.OrderByDescending(h => h.Fused).Take(topK).ToList();

            // Safety net: a very strong keyword match (a rare word like "restore" or "E-4021")
            // must not be lost in the fusion, so it takes the last place if it isn't already in
            var bestKeyword = hits.MaxBy(h => h.KeywordScore);
            if (bestKeyword is not null && bestKeyword.KeywordScore >= MinKeywordScore && !top.Contains(bestKeyword))
                top[^1] = bestKeyword;

            return top;
        }

        var ordered = mode switch
        {
            SearchMode.Vector => hits.OrderByDescending(h => h.Score),
            _ => hits.OrderByDescending(h => h.KeywordScore)
        };

        return ordered.Take(topK).ToList();
    }

    // [0.2, 0.9, 0.5] → [3, 1, 2]
    private static int[] RankOf(double[] scores)
    {
        var ranks = new int[scores.Length];
        var order = scores.Select((s, i) => (s, i)).OrderByDescending(x => x.s).ToList();
        for (int r = 0; r < order.Count; r++) ranks[order[r].i] = r + 1;
        return ranks;
    }
}