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

    // ===== учебные сессии =====

    /// <summary>Все сессии пользователя (для восстановления активной и для истории). Ошибка → пустой список.</summary>
    public async Task<IReadOnlyList<SessionApiResponse>> GetSessionsAsync()
    {
        try
        {
            using var req = Authorized(HttpMethod.Get, "/sessions");
            using var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return Array.Empty<SessionApiResponse>();

            return await resp.Content.ReadFromJsonAsync<List<SessionApiResponse>>(Json)
                   ?? new List<SessionApiResponse>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] GET /sessions: {ex.Message}");
            return Array.Empty<SessionApiResponse>();
        }
    }

    /// <summary>
    /// Стартует сессию по предмету. Возвращает кортеж (сессия, текст ошибки):
    /// при успехе — (session, null), при неудаче — (null, сообщение).
    /// Сервер вернёт 409, если у юзера уже есть активная/приостановленная сессия —
    /// текст ошибки достаём из тела через <see cref="ReadErrorAsync"/>.
    /// </summary>
    public async Task<(SessionApiResponse? session, string? error)> StartSessionAsync(int subjectId)
    {
        try
        {
            using var req = Authorized(HttpMethod.Post, "/sessions");
            req.Content = JsonContent.Create(new { subjectId });
            using var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
                return (await resp.Content.ReadFromJsonAsync<SessionApiResponse>(Json), null);

            return (null, await ReadErrorAsync(resp));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] POST /sessions: {ex.Message}");
            return (null, "Не удалось связаться с сервером");
        }
    }

    /// <summary>Пауза сессии (POST /sessions/{id}/pause). null = не получилось.</summary>
    public Task<SessionApiResponse?> PauseSessionAsync(int sessionId) => SessionActionAsync(sessionId, "pause");

    /// <summary>Продолжить после паузы (POST /sessions/{id}/resume). null = не получилось.</summary>
    public Task<SessionApiResponse?> ResumeSessionAsync(int sessionId) => SessionActionAsync(sessionId, "resume");

    /// <summary>Завершить сессию (POST /sessions/{id}/complete). null = не получилось.</summary>
    public Task<SessionApiResponse?> CompleteSessionAsync(int sessionId) => SessionActionAsync(sessionId, "complete");

    /// <summary>Общая «ручка» для pause/resume/complete — все три это POST без тела по схожему адресу.</summary>
    private async Task<SessionApiResponse?> SessionActionAsync(int sessionId, string action)
    {
        try
        {
            using var req = Authorized(HttpMethod.Post, $"/sessions/{sessionId}/{action}");
            using var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<SessionApiResponse>(Json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] POST /sessions/{sessionId}/{action}: {ex.Message}");
            return null;
        }
    }

    // ===== рейтинг =====

    /// <summary>
    /// Запрашивает топ пользователей за период (<paramref name="period"/> = daily|weekly|monthly).
    /// Эндпоинт требует JWT (RequireAuthorization на сервере) — токен подставляет <see cref="Authorized"/>.
    /// При любой ошибке (нет сети, не 2xx, битый JSON) возвращает пустой список, чтобы UI не падал.
    /// </summary>
    public async Task<IReadOnlyList<LeaderboardEntryResponse>> GetLeaderboardAsync(string period)
    {
        try
        {
            using var req = Authorized(HttpMethod.Get, $"/leaderboard/{period}");
            using var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return Array.Empty<LeaderboardEntryResponse>();

            return await resp.Content.ReadFromJsonAsync<List<LeaderboardEntryResponse>>(Json)
                   ?? new List<LeaderboardEntryResponse>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] GET /leaderboard/{period}: {ex.Message}");
            return Array.Empty<LeaderboardEntryResponse>();
        }
    }

    /// <summary>
    /// Собирает HTTP-запрос и, если в <see cref="IAuthService"/> есть токен,
    /// добавляет заголовок <c>Authorization: Bearer &lt;jwt&gt;</c>.
    /// Через неё проходят все защищённые вызовы (задачи, предметы, сессии, рейтинг).
    /// </summary>
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
