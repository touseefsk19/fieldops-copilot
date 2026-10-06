namespace FieldOps.Api.Models;

// One line in the audit log: who did what, with which input, and what happened
public class AuditEntry
{
    public int Id {get; set;}
    public DateTime AtUtc {get; set;} = DateTime.UtcNow;
    public string User {get; set;} = "";
    public string Action {get; set;} = "";
    public string Input {get; set;} = "";
    public string Result {get; set;} = "";

}