var builder = WebApplication.CreateBuilder(args);

// Logs as JSON lines: easy to search later in Azure App Insights
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

// Services the app needs (dependency injection container)
builder.Services.AddOpenApi();       // generates the API description document
builder.Services.AddHealthChecks();  // lets load balancers ask "are you alive?"

var app = builder.Build();

// Only expose the OpenAPI document on your own machine
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                // serves /openapi/v1.json
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");      // GET /health -> "Healthy"

// GET /api/info -> small JSON showing config + environment
app.MapGet("/api/info", (IConfiguration config, ILogger<Program> logger) =>
{
    logger.LogInformation("Info requested");
    return Results.Ok(new
    {
        name = config["App:Name"],
        version = "0.1.0",
        environment = app.Environment.EnvironmentName
    });
})
.WithName("GetInfo");

app.Run();