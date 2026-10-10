namespace FieldOps.Mobile.Models;

// Same shapes as the API (RequestDto, AgentRequest/Response, /api/me, approve result)
public record RequestDto(int Id, string Title, string? Description, string Equipment, string Status, DateTime CreatedAtUtc);
public record ApproveResponse(int Id, string Status);
public record AgentRequest(string Message);
public record AgentResponse(string Reply, List<string> ToolsCalled);
public record MeResponse(string? Name, List<string> Roles);