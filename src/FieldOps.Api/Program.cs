using FieldOps.Api.Data;
using FieldOps.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logs as JSON lines: easy to search later in Azure App Insights
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Register the DbContext. Lifetime = Scoped: one instance per HTTP request
builder.Services.AddDbContext<FieldOpsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FieldOps")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Apply pending migrations at startup (development only)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FieldOpsDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();

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

app.MapRequestEndpoints();

app.Run();