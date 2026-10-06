using FieldOps.Api.Agent;
using FieldOps.Api.Security;
using Microsoft.Extensions.AI;

namespace FieldOps.Api.Endpoints;

public record AgentRequest(string Message);
public record AgentResponse(string Reply, List<string> ToolsCalled);

public static class AgentChatEndpoints
{
    private const int MaxMessageLength = 500;

    private const string AgentPrompt =
        "You are the FieldOps assistant for maintenance staff. " +
        "Use the tools to look up spare parts, raise maintenance requests and check request status. " +
        "You cannot approve requests: only a supervisor can, in the FieldOps app. " +
        "Base your answer only on what the tools return. Answer in at most 3 sentences.";

    public static void MapAgentChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/agent", async (AgentRequest request, FieldOpsTools tools, IChatClient chat,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > MaxMessageLength)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["message"] = [$"Message is required and must be at most {MaxMessageLength} characters."]
                });

            // Wrap the normal chat client so it can run tools: model asks → our code runs the method → result goes back
            var agent = new ChatClientBuilder(chat)
                .UseFunctionInvocation(configure: f => f.MaximumIterationsPerRequest = 3)   // no endless tool loops
                .Build();

            var options = new ChatOptions
            {
                Temperature = 0f,
                // The ONLY 3 actions the model may take. No approve, no delete, no SQL.
                Tools =
                [
                    AIFunctionFactory.Create(tools.LookupStockAsync),
                    AIFunctionFactory.Create(tools.CreateRequestAsync),
                    AIFunctionFactory.Create(tools.CheckRequestStatusAsync)
                ]
            };

            List<ChatMessage> messages =
            [
                new(ChatRole.System, AgentPrompt),
                new(ChatRole.User, PiiRedactor.Redact(request.Message))
            ];

            var response = await agent.GetResponseAsync(messages, options, ct);

            // Which tools did the model actually call? (also recorded in the audit log)
            var toolsCalled = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionCallContent>()
                .Select(c => c.Name)
                .ToList();

            // Output filter: never return personal data, and never return an empty answer
            var reply = string.IsNullOrWhiteSpace(response.Text)
                ? "I can't do that here. Approvals are made by a supervisor in the FieldOps app."
                : PiiRedactor.Redact(response.Text);

            return Results.Ok(new AgentResponse(reply, toolsCalled));
        })
        .RequireAuthorization()
        .RequireRateLimiting("ai")
        .WithTags("AI");
    }
}