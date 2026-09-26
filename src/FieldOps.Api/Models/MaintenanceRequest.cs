namespace FieldOps.Api.Models;

// An entity = a class EF Core maps to a database table
public class MaintenanceRequest
{
    public int Id { get; set; }                      // primary key (EF convention: "Id")
    public string Title { get; set; } = "";
    public string? Description { get; set; }         // ? = optional (nullable column)
    public string Equipment { get; set; } = "";
    public RequestStatus Status { get; set; } = RequestStatus.Open;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum RequestStatus { Open, Approved, InProgress, Closed }