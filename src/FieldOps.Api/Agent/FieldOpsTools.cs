using System.ComponentModel;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using FieldOps.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Agent;

// The only actions an AI agent will be allowed to take (part 2 hands these to the model).
// Each one is small, validated, and written to the audit log.
public class FieldOpsTools(FieldOpsDbContext db, IHttpContextAccessor http)
{
    private string CurrentUser => http.HttpContext?.User.Identity?.Name ?? "unknown";

    [Description("Look up spare parts in the warehouse by part number or name. Returns quantity and bin.")]
    public async Task<string> LookupStockAsync(
        [Description("A part number like SK-200, or a word from the part name like 'seal'")] string query,
        CancellationToken ct = default)
    {
        var q = query.Trim().ToLower();
        var parts = await db.SpareParts.AsNoTracking()
            .Where(p => p.PartNumber.ToLower() == q || p.Name.ToLower().Contains(q))
            .OrderBy(p => p.PartNumber)
            .Take(5)
            .ToListAsync(ct);

        var result = parts.Count == 0
            ? $"No part matches '{query}'."
            : string.Join("; ", parts.Select(p => $"{p.PartNumber} {p.Name}: {p.Quantity} in bin {p.Bin}"));

        await AuditAsync("LookupStock", query, result, ct);
        return result;
    }

    [Description("Raise a maintenance request. It is created as PendingApproval; a supervisor must approve it.")]
    public async Task<string> CreateRequestAsync(
        [Description("A short title, e.g. 'Replace CP-200 seal'")] string title,
        [Description("The equipment tag, e.g. 'P-101'")] string equipment,
        [Description("Optional details")] string? description = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(equipment))
            return "Title and equipment are required.";

        var request = new MaintenanceRequest
        {
            Title = PiiRedactor.Redact(title.Trim()),                                            // mask at the point of entry,
            Equipment = equipment.Trim(),                                                        
            Description = description is null ? null : PiiRedactor.Redact(description),          // so it's never stored
            Status = RequestStatus.PendingApproval   // never Approved: a human decides that
        };
        db.MaintenanceRequests.Add(request);
        await db.SaveChangesAsync(ct);

        var result = $"Request {request.Id} created and waiting for supervisor approval.";
        await AuditAsync("CreateRequest", $"{title} | {equipment}", result, ct);
        return PiiRedactor.Redact(result);
    }

    [Description("Check the status of a maintenance request by its number.")]
    public async Task<string> CheckRequestStatusAsync(
        [Description("The request number")] int requestId,
        CancellationToken ct = default)
    {
        var r = await db.MaintenanceRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == requestId, ct);
        var result = r is null
            ? $"Request {requestId} not found."
            : $"Request {r.Id} '{r.Title}' is {r.Status}.";

        await AuditAsync("CheckRequestStatus", requestId.ToString(), result, ct);
        return result;
    }

    // Every tool call leaves a trace; personal data is masked before it's stored
    private async Task AuditAsync(string action, string input, string result, CancellationToken ct)
    {
        db.AuditEntries.Add(new AuditEntry
        {
            User = CurrentUser,
            Action = action,
            Input = PiiRedactor.Redact(input),
            Result = result
        });
        await db.SaveChangesAsync(ct);
    }
}