using FieldOps.Api.Contracts;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class RequestEndpoints
{
    // Extension method: lets Program.cs call app.MapRequestEndpoints()
    public static void MapRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requests").WithTags("Requests");

        // LIST: newest first. AsNoTracking = read-only, faster (EF doesn't track changes)
        group.MapGet("/", async (FieldOpsDbContext db, CancellationToken ct) =>
        {
            var items = await db.MaintenanceRequests
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync(ct);
            return Results.Ok(items.Select(ToDto));
        });

        // GET ONE
        group.MapGet("/{id:int}", async (int id, FieldOpsDbContext db, CancellationToken ct) =>
        {
            var r = await db.MaintenanceRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return r is null ? Results.NotFound() : Results.Ok(ToDto(r));
        });

        // CREATE
        group.MapPost("/", async (CreateRequestDto dto, FieldOpsDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Equipment))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["request"] = ["Title and Equipment are required."]
                });

            var entity = new MaintenanceRequest
            {
                Title = dto.Title.Trim(),
                Description = dto.Description,
                Equipment = dto.Equipment.Trim()
            };
            db.MaintenanceRequests.Add(entity);   // EF now tracks it as "Added"
            await db.SaveChangesAsync(ct);         // INSERT runs here; Id gets filled in
            return Results.Created($"/api/requests/{entity.Id}", ToDto(entity));
        });

        // UPDATE STATUS
        group.MapPatch("/{id:int}/status", async (int id, UpdateStatusDto dto, FieldOpsDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<RequestStatus>(dto.Status, ignoreCase: true, out var status))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["status"] = ["Use Open, Approved, InProgress or Closed."]
                });

            var entity = await db.MaintenanceRequests.FindAsync(new object[] { id }, ct);
            if (entity is null) return Results.NotFound();

            entity.Status = status;                // EF detects the change (tracked entity)
            await db.SaveChangesAsync(ct);          // UPDATE runs here
            return Results.Ok(ToDto(entity));
        });

        // DELETE: ExecuteDeleteAsync runs one DELETE statement without loading the row first
        group.MapDelete("/{id:int}", async (int id, FieldOpsDbContext db, CancellationToken ct) =>
        {
            var deleted = await db.MaintenanceRequests.Where(r => r.Id == id).ExecuteDeleteAsync(ct);
            return deleted == 0 ? Results.NotFound() : Results.NoContent();
        });
    }

    private static RequestDto ToDto(MaintenanceRequest r) =>
        new(r.Id, r.Title, r.Description, r.Equipment, r.Status.ToString(), r.CreatedAtUtc);
}