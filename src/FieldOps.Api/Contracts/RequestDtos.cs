namespace FieldOps.Api.Contracts;

// DTOs = the shapes the API accepts/returns. Never expose entities directly.
public record CreateRequestDto(string Title, string? Description, string Equipment);
public record UpdateStatusDto(string Status);
public record RequestDto(int Id, string Title, string? Description, string Equipment, string Status, DateTime CreatedAtUtc);