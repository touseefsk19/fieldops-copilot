using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FieldOps.Mobile.Models;

namespace FieldOps.Mobile.Services;

// One exception type the UI can show as-is: plain words, no stack traces
public class ApiException(string message) : Exception(message);

public class FieldOpsApi(HttpClient http, TokenStore tokens)
{
    public async Task<AskResponse> AskAsync(string question, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ask")
        {
            Content = JsonContent.Create(new AskRequest(question))
        };
        AddToken(request);

        using var response = await SendAsync(request, ct);
        return await response.Content.ReadFromJsonAsync<AskResponse>(ct)
               ?? throw new ApiException("The server sent an empty answer.");
    }

    private void AddToken(HttpRequestMessage request)
    {
        var token = tokens.Token;
        if (string.IsNullOrEmpty(token))
            throw new ApiException("No token yet. Open the Settings tab and paste one.");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // Every call goes through here: network errors and status codes become friendly messages
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
            HttpStatusCode.Forbidden => "Your role can't do this.",
            HttpStatusCode.TooManyRequests => "Too many questions this minute. Wait a moment and try again.",
            HttpStatusCode.BadRequest => "The question must be 1–500 characters.",
            _ => $"Server error ({(int)response.StatusCode})."
        };
        response.Dispose();
        throw new ApiException(message);
    }
}