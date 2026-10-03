using System.Text.Json;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using FieldOps.Api.Rag;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public record IngestResponse(int Files, int Chunks);

public static class ManualEndpoints
{
    public static void MapManualEndpoints(this IEndpointRouteBuilder app)
    {
        // Supervisor only: read every .md file in Manuals/, split it into sections, embed, store
        app.MapPost("/api/manuals/ingest", async (
            FieldOpsDbContext db, OllamaEmbedder embedder, IWebHostEnvironment env, CancellationToken ct) =>
        {
            var folder = Path.Combine(env.ContentRootPath, "Manuals");
            var files = Directory.GetFiles(folder, "*.md");
            int total = 0;

            foreach (var file in files)
            {
                var source = Path.GetFileName(file);

                // Re-ingesting a file replaces its old chunks, so running this twice never duplicates
                await db.ManualChunks.Where(c => c.Source == source).ExecuteDeleteAsync(ct);

                var markdown = await File.ReadAllTextAsync(file, ct);
                foreach (var chunk in ManualChunker.Split(markdown))
                {
                    var vector = await embedder.EmbedAsync(chunk.Text, ct);
                    db.ManualChunks.Add(new ManualChunk
                    {
                        Source = source,
                        Section = chunk.Section,
                        Text = chunk.Text,
                        EmbeddingJson = JsonSerializer.Serialize(vector)
                    });
                    total++;
                }
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(new IngestResponse(files.Length, total));
        })
        .RequireAuthorization("Supervisor")
        .WithTags("Manuals");

        // Any signed-in user: list what's in the knowledge base (no text, no vectors)
        app.MapGet("/api/manuals", async (FieldOpsDbContext db, CancellationToken ct) =>
        {
            var list = await db.ManualChunks
                .OrderBy(c => c.Source).ThenBy(c => c.Id)
                .Select(c => new { c.Id, c.Source, c.Section })
                .ToListAsync(ct);
            return Results.Ok(list);
        })
        .RequireAuthorization()
        .WithTags("Manuals");
    }
}