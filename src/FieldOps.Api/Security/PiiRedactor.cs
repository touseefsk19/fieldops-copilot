using System.Text.RegularExpressions;

namespace FieldOps.Api.Security;

// Masks personal data before it is logged, embedded or sent to a model
public static partial class PiiRedactor
{
    [GeneratedRegex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")]
    private static partial Regex Email();

    // Digits with optional spaces/hyphens, e.g. "+91 98765 43210"
    [GeneratedRegex(@"\+?\d[\d\s-]{8,}\d")]
    private static partial Regex LongNumber();

    public static string Redact(string text)
    {
        var noEmails = Email().Replace(text, "[EMAIL]");

        // Only numbers with 10+ digits count as phone/account numbers, so "2026-10-03" and "CP-200" stay
        return LongNumber().Replace(noEmails, m => m.Value.Count(char.IsDigit) >= 10 ? "[PHONE]" : m.Value);
    }
}