using System.Text.Json;
using FieldOps.Api.Data;
using FieldOps.Api.Rag;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace FieldOps.Api.Endpoints;

public record AskRequest(string Question);
public record Citation(int Number, string Source, string Section, double Score);
public record AskResponse(string Answer, List<Citation> Citations, long? InputTokens, long? OutputTokens);

public static class AskEndpoints
{
    private const double MinScore = 0.35; // the gate: below this, no section is relevant enough
    private const int TopK = 3;           // how many sections go into the prompt

    public static void MapAskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ask", async (AskRequest request, FieldOpsDbContext db, OllamaEmbedder embedder,
            IChatClient chat, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["question"] = ["Question is required."]
                });

            // 1. RETRIEVE: embed the question, score every section, keep the best 3
            var questionVector = await embedder.EmbedAsync(request.Question, ct);
            var chunks = await db.ManualChunks.AsNoTracking().ToListAsync(ct);

            var top = chunks
                .Select(c => new
                {
                    Chunk = c,
                    Score = VectorMath.Cosine(questionVector, JsonSerializer.Deserialize<float[]>(c.EmbeddingJson)!)
                })
                .OrderByDescending(x => x.Score)
                .Take(TopK)
                .ToList();

            // 2. GATE: no good evidence → no LLM call, no made-up answer
            if (top.Count == 0 || top[0].Score < MinScore)
                return Results.Ok(new AskResponse(
                    "I can't find this in the manuals I have. Please check with your supervisor.", [], null, null));

            // 3. AUGMENT: numbered sources, so the model can cite [1], [2], [3]
            var sources = string.Join("\n\n", top.Select((x, i) =>
                $"[{i + 1}] ({x.Chunk.Source}, section {x.Chunk.Section}) {x.Chunk.Text}"));

            List<ChatMessage> messages =
            [
                new(ChatRole.System,
                    "You are FieldOps Copilot, an assistant for maintenance technicians. " +
                    "Answer using ONLY the numbered sources in the user message and cite them like [1]. " +
                    "If the sources do not contain the answer, say \"I don't know\". Answer in at most 4 sentences."),
                new(ChatRole.User, $"Sources:\n{sources}\n\nQuestion: {request.Question}")
            ];

            // 4. GENERATE
            var response = await chat.GetResponseAsync(messages, new ChatOptions { Temperature = 0f }, ct);

            var citations = top
                .Select((x, i) => new Citation(i + 1, x.Chunk.Source, x.Chunk.Section, Math.Round(x.Score, 3)))
                .ToList();

            return Results.Ok(new AskResponse(
                response.Text,
                citations,
                response.Usage?.InputTokenCount,
                response.Usage?.OutputTokenCount));
        })
        .RequireAuthorization()
        .WithTags("AI");
    }
}