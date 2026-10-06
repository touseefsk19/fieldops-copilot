using System.Security.Claims;
using FieldOps.Api.Agent;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public record ToolRequestDto(string Title, string Equipment, string? Description);

public static class AgentEndpoints
{
    public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        // The 3 tools, callable directly today; in part 2 the model calls the same methods
        var tools = app.MapGroup("/api/tools").WithTags("Agent tools").RequireAuthorization();

        tools.MapGet("/stock", async (string q, FieldOpsTools t, CancellationToken ct) =>
            Results.Ok(new { result = await t.LookupStockAsync(q, ct) }));

        tools.MapPost("/requests", async (ToolRequestDto dto, FieldOpsTools t, CancellationToken ct) =>
            Results.Ok(new { result = await t.CreateRequestAsync(dto.Title, dto.Equipment, dto.Description, ct) }));

        tools.MapGet("/requests/{id:int}", async (int id, FieldOpsTools t, CancellationToken ct) =>
            Results.Ok(new { result = await t.CheckRequestStatusAsync(id, ct) }));

        // Human in the loop: only a Supervisor can turn PendingApproval into Approved
        app.MapPost("/api/requests/{id:int}/approve", async (int id, FieldOpsDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var r = await db.MaintenanceRequests.FindAsync(new object[] { id }, ct);
            if (r is null) return Results.NotFound();
            if (r.Status != RequestStatus.PendingApproval)
                return Results.Problem($"Request {id} is {r.Status}, not PendingApproval.", statusCode: StatusCodes.Status409Conflict);

            r.Status = RequestStatus.Approved;
            db.AuditEntries.Add(new AuditEntry
            {
                User = user.Identity?.Name ?? "unknown",
                Action = "ApproveRequest",
                Input = id.ToString(),
                Result = "Approved"
            });
            await db.SaveChangesAsync(ct);   // status change and audit line saved together
            return Results.Ok(new { id, status = r.Status.ToString() });
        })
        .RequireAuthorization("Supervisor")
        .WithTags("Agent tools");

        // The audit log: newest first, Supervisor only
        app.MapGet("/api/audit", async (FieldOpsDbContext db, CancellationToken ct) =>
            Results.Ok(await db.AuditEntries.AsNoTracking().OrderByDescending(a => a.Id).Take(50).ToListAsync(ct)))
        .RequireAuthorization("Supervisor")
        .WithTags("Agent tools");
    }
}