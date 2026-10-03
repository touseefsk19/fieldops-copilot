using System.Security.Claims;
using FieldOps.Api.Rag;
using Microsoft.Extensions.AI;

namespace FieldOps.Api.Endpoints;

public record AskRequest(string Question);
public record Citation(int Number, string Source, string Section, double Score);
public record AskResponse(string Answer, List<Citation> Citations, long? InputTokens, long? OutputTokens);

public static class AskEndpoints
{
    private const int TopK = 3; // at most 3 sections go into the prompt

    public static void MapAskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ask", async (AskRequest request, Retriever retriever, IChatClient chat,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["question"] = ["Question is required."]
                });

            // 1. RETRIEVE: only sections this user may see, best 3 first
            var hits = await retriever.SearchAsync(request.Question, user.IsInRole("supervisor"), TopK, ct);

            // 2. GATE + FILTER: only sections that pass the bar reach the model (weak ones confuse it)
            var top = hits.Where(h => h.Score >= Retriever.MinScore).ToList();
            if (top.Count == 0)
                return Results.Ok(new AskResponse(
                    "I can't find this in the manuals I have. Please check with your supervisor.", [], null, null));

            // 3. AUGMENT: numbered sources, so the model can cite [1], [2], [3]
            var sources = string.Join("\n\n", top.Select((h, i) =>
                $"[{i + 1}] ({h.Chunk.Source}, section {h.Chunk.Section}) {h.Chunk.Text}"));

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
                .Select((h, i) => new Citation(i + 1, h.Chunk.Source, h.Chunk.Section, Math.Round(h.Score, 3)))
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