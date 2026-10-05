using System.Security.Claims;
using FieldOps.Api.Rag;
using FieldOps.Api.Security;
using Microsoft.Extensions.AI;

namespace FieldOps.Api.Endpoints;

public record AskRequest(string Question);
public record Citation(int Number, string Source, string Section, double Score);
public record AskResponse(string Answer, List<Citation> Citations, long? InputTokens, long? OutputTokens);

public static class AskEndpoints
{
    private const int TopK = 3;               // at most 3 sections go into the prompt
    private const int MaxQuestionLength = 500; // long inputs are a classic injection and cost vector

    public static void MapAskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ask", async (AskRequest request, Retriever retriever, IChatClient chat,
            ClaimsPrincipal user, ILogger<Program> logger, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question) || request.Question.Length > MaxQuestionLength)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["question"] = [$"Question is required and must be at most {MaxQuestionLength} characters."]
                });

            // 0. REDACT: personal data never reaches the logs, the embedding model or the chat model
            var question = PiiRedactor.Redact(request.Question);
            logger.LogInformation("Ask by {User}: {Question}", user.Identity?.Name, question);

            // 1. RETRIEVE: only sections this user may see, best 3 first
            var hits = await retriever.SearchAsync(question, user.IsInRole("supervisor"), TopK, ct);

                        // 2. GATE + FILTER: keep a section if its meaning OR its keywords match strongly
            var top = hits
                .Where(h => h.Score >= Retriever.MinScore || h.KeywordScore >= Retriever.MinKeywordScore)
                .ToList();
            if (top.Count == 0)
                return Results.Ok(new AskResponse(
                    "I can't find this in the manuals I have. Please check with your supervisor.", [], null, null));

            // 3. AUGMENT: fixed system prompt, sources fenced as data, question in the user message
            var messages = PromptBuilder.Build(question, top);

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
        .RequireRateLimiting("ai")
        .WithTags("AI");
    }
}