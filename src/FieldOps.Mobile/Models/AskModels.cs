namespace FieldOps.Mobile.Models;

// Same shapes as FieldOps.Api AskEndpoints, so JSON maps 1:1
public record AskRequest(string Question);
public record Citation(int Number, string Source, string Section, double Score);
public record AskResponse(string Answer, List<Citation> Citations, long? InputTokens, long? OutputTokens);