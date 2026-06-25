using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using itmodd.Models;

namespace itmodd.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<IReadOnlyList<SubjectApiResponse>> GetSubjectsAsync()
    {
        try
        {
            using var req = Authorized(HttpMethod.Get, "/subjects");
            using var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return Array.Empty<SubjectApiResponse>();

            return await resp.Content.ReadFromJsonAsync<List<SubjectApiResponse>>(Json)
                   ?? new List<SubjectApiResponse>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] GET /subjects: {ex.Message}");
            return Array.Empty<SubjectApiResponse>();
        }
    }

    public async Task<SubjectApiResponse?> CreateSubjectAsync(string name, string color)
    {
        try
        {
            using var req = Authorized(HttpMethod.Post, "/subjects");
            req.Content = JsonContent.Create(new { subjectName = name, description = (string?)null, color });
            using var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<SubjectApiResponse>(Json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] POST /subjects: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> CreateTaskAsync(string title, string? description, int subjectId, DateTime deadline)
    {
        if (string.IsNullOrWhiteSpace(_auth.Token))
            return "Нужно войти в аккаунт";

        try
        {
            using var req = Authorized(HttpMethod.Post, "/tasks");
            req.Content = JsonContent.Create(new
            {
                title,
                description,
                subjectId,
                deadlineAt = deadline
            });

            using var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
                return null;

            return await ReadErrorAsync(resp);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] POST /tasks: {ex.Message}");
            return "Не удалось связаться с сервером";
        }
    }

    private HttpRequestMessage Authorized(HttpMethod method, string path)
    {
        var req = new HttpRequestMessage(method, path);
        var token = _auth.Token;
        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    // тело ошибки имеет вид {"error":"..."}
    private static async Task<string> ReadErrorAsync(HttpResponseMessage resp)
    {
        try
        {
            var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
            if (doc.TryGetProperty("error", out var err))
                return err.GetString() ?? "Ошибка сохранения";
        }
        catch { /* не json */ }

        return $"Ошибка ({(int)resp.StatusCode})";
    }
}
