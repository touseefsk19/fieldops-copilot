using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using System.Security.Claims;
using FieldOps.Api.Data;
using FieldOps.Api.Endpoints;
using FieldOps.Api.Rag;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logs as JSON lines: easy to search later in Azure App Insights
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

// Authentication: validate JWT bearer tokens (settings come from configuration)
builder.Services.AddAuthentication().AddJwtBearer();

// Authorization: named rules that endpoints can require
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Supervisor", policy => policy.RequireRole("supervisor"));

// AI: one IChatClient for the whole app, pointed at Ollama's OpenAI-compatible endpoint
builder.Services.AddSingleton<IChatClient>(sp =>
{
   var config = sp.GetRequiredService<IConfiguration>();
   var openAi = new OpenAIClient(
    new ApiKeyCredential("ollama"),  // Ollama ignores the key, but the client requires one
    new OpenAIClientOptions {Endpoint = new Uri(config["Ai:Endpoint"]!)
    });
   return openAi.GetChatClient(config["Ai:Model"]!).AsIChatClient(); 
});

// AI: the embedding client, a typed HttpClient pointed at Ollama
builder.Services.AddHttpClient<OllamaEmbedder>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Ai:OllamaBase"]!));

// Register the DbContext. Lifetime = Scoped: one instance per HTTP request
builder.Services.AddDbContext<FieldOpsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FieldOps")));

// RAG: retrieval shared by /api/ask and /api/eval
builder.Services.AddScoped<Retriever>();

var app = builder.Build();

// Errors: unhandled exceptions → 500 ProblemDetails (details go to logs, never to the caller)
app.UseExceptionHandler();
// Empty error responses (e.g. 404) → ProblemDetails too
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Development only: an endpoint that throws, to prove the error handling works
    app.MapGet("/api/debug/throw", () =>
    {
        throw new InvalidOperationException("Test failure from /api/");
    });

    // Apply pending migrations at startup (development only)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FieldOpsDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/api/info", (IConfiguration config, ILogger<Program> logger) =>
{
    logger.LogInformation("Info requested");
    return Results.Ok(new
    {
        name = config["App:Name"],
        version = "0.2.0",
        environment = app.Environment.EnvironmentName
    });
})
.WithName("GetInfo");

// Shows what the API knows about the caller: handy for testing tokens
app.MapGet("/api/me", (ClaimsPrincipal user) => Results.Ok(new
{
    name = user.Identity?.Name,
    roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value),
    claims = user.Claims.Select(c => new { c.Type, c.Value })
}))
.RequireAuthorization();
app.MapRequestEndpoints();
app.MapAskEndpoints();
app.MapManualEndpoints();
app.MapEvalEndpoints();

app.Run();
// Lets the test project start this API in memory (WebApplicationFactory<Program>)
public partial class Program{}