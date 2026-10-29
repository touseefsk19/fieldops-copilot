using System.Text.Json;
using FieldOps.Mobile.Models;

namespace FieldOps.Mobile.Services;

// The last list the API gave us, saved in the app's private folder.
// Titles are already PII-redacted by the API. Part 3b clears this on sign-out.
public class RequestsCache
{
    private readonly string path = Path.Combine(FileSystem.AppDataDirectory, "requests-cache.json");

    public async Task SaveAsync(List<RequestDto> items)
    {
        var snapshot = new CachedRequests(DateTime.UtcNow, items);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(snapshot));
    }

    public async Task<CachedRequests?> LoadAsync()
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<CachedRequests>(await File.ReadAllTextAsync(path));
        }
        catch (JsonException)
        {
            return null;   // damaged file: act as if there is no cache
        }
    }
}

public record CachedRequests(DateTime SavedAtUtc, List<RequestDto> Items);