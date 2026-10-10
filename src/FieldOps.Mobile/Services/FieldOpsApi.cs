using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FieldOps.Mobile.Models;

namespace FieldOps.Mobile.Services;

// One exception type the UI can show as-is: plain words, no stack traces
public class ApiException(string message) : Exception(message);

public class FieldOpsApi(HttpClient http, TokenStore tokens)
{
    // Every endpoint is now one line
    public Task<AskResponse> AskAsync(string question, CancellationToken ct = default) =>
        SendJsonAsync<AskResponse>(HttpMethod.Post, "/api/ask", new AskRequest(question), ct);

    public Task<MeResponse> GetMeAsync(CancellationToken ct = default) =>
        SendJsonAsync<MeResponse>(HttpMethod.Get, "/api/me", null, ct);

    public Task<List<RequestDto>> GetRequestsAsync(CancellationToken ct = default) =>
        SendJsonAsync<List<RequestDto>>(HttpMethod.Get, "/api/requests", null, ct);

    public Task<ApproveResponse> ApproveAsync(int id, CancellationToken ct = default) =>
        SendJsonAsync<ApproveResponse>(HttpMethod.Post, $"/api/requests/{id}/approve", null, ct);

    public Task<AgentResponse> SendToAgentAsync(string message, CancellationToken ct = default) =>
        SendJsonAsync<AgentResponse>(HttpMethod.Post, "/api/agent", new AgentRequest(message), ct);

    // The one path every call takes: token → send → friendly errors → read JSON as T
    private async Task<T> SendJsonAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        AddToken(request);

        using var response = await SendAsync(request, ct);
        return await response.Content.ReadFromJsonAsync<T>(ct)
               ?? throw new ApiException("The server sent an empty answer.");
    }

    private void AddToken(HttpRequestMessage request)
    {
        var token = tokens.Token;
        if (string.IsNullOrEmpty(token))
            throw new ApiException("No token yet. Open the Settings tab and paste one.");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Can't reach the API. Is it running on port 5266?");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiException("The answer took too long. Try again.");
        }

        if (response.IsSuccessStatusCode) return response;

        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Token missing or expired. Paste a fresh one in Settings.",
            HttpStatusCode.Forbidden => "Only a supervisor can do this.",
            HttpStatusCode.NotFound => "Not found. Refresh the list.",
            HttpStatusCode.Conflict => "This request is no longer waiting for approval. Refresh the list.",
            HttpStatusCode.TooManyRequests => "Too many requests this minute. Wait a moment and try again.",
            HttpStatusCode.BadRequest => "The text must be 1–500 characters.",
            _ => $"Server error ({(int)response.StatusCode})."
        };
        response.Dispose();
        throw new ApiException(message);
    }
}