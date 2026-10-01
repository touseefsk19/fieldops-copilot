using Microsoft.Extensions.AI;

namespace FieldOps.Api.Endpoints;

public record AskRequest(string Question);
public record AskResponse(string Answer, long? InputTokens, long? OutputTokens);

public static class AskEndpoints
{
    public static void MapAskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ask", async (AskRequest request, IChatClient chat, CancellationToken ct) =>
        {
            if(string.IsNullOrWhiteSpace(request.Question))
               return Results.ValidationProblem(new Dictionary<string, string[]>
               {
                  ["question"] = ["Question is required."] 
               });

            List<ChatMessage> messages =
            [
                new(ChatRole.System, "You are FieldOps Copilot, an assistant for maintenance technicians. Answer in at most 3 sentences. If you are not sure, say so."),
                new(ChatRole.User, request.Question)
            ];

            var response = await chat.GetResponseAsync(messages, new ChatOptions {Temperature = 0f }, ct);

            return Results.Ok(new AskResponse(
                response.Text,
                response.Usage?.InputTokenCount,
                response.Usage?.OutputTokenCount
            ));
        })
        .RequireAuthorization()
        .WithTags("AI");
    }
}