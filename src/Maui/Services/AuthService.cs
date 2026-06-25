using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using itmodd.Models;
using Microsoft.Maui.Storage;

namespace itmodd.Services;

// Авторизация через API: /login, /register. Токен держим в памяти и в хранилище.
public class AuthService : IAuthService
{
    private const string TokenKey = "jwt";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private string? _token;
    private int? _currentUserId;
    private string? _currentUsername;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public string? Token => _token;
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_token);
    /// <summary>Id текущего пользователя, вытащенный из JWT. null — если не залогинен/токен битый.</summary>
    public int? CurrentUserId => _currentUserId;

    /// <summary>Имя текущего пользователя из JWT.</summary>
    public string? CurrentUsername => _currentUsername;

    /// <summary>
    /// Единая точка установки токена (логин/восстановление/логаут).
    /// Заодно сразу разбирает payload JWT и кэширует userId/username,
    /// чтобы не парсить токен каждый раз.
    /// </summary>
    private void SetToken(string? token)
    {
        _token = token;
        (_currentUserId, _currentUsername) = ParseToken(token);
    }

    public async Task<bool> TryRestoreAsync()
    {
        SetToken(await LoadTokenAsync());
        return IsAuthenticated;
    }

    public Task<AuthResult> LoginAsync(string email, string password) =>
        AuthenticateAsync("/login", new { email, password });

    public Task<AuthResult> RegisterAsync(string username, string email, string password) =>
        AuthenticateAsync("/register", new { username, email, password });

    public async Task LogoutAsync()
    {
        SetToken(null);
        try { SecureStorage.Default.Remove(TokenKey); } catch { /* игнор */ }
        Preferences.Default.Remove(TokenKey);
        await Task.CompletedTask;
    }

    // общий путь для /login и /register — оба возвращают токен
    private async Task<AuthResult> AuthenticateAsync(string path, object body)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(path, body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response);
                return new AuthResult(false, error);
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            if (auth is null || string.IsNullOrWhiteSpace(auth.Token))
                return new AuthResult(false, "Пустой ответ сервера");

            SetToken(auth.Token);
            await SaveTokenAsync(auth.Token);
            return new AuthResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] {path}: {ex.Message}");
            return new AuthResult(false, "Не удалось связаться с сервером");
        }
    }

    // тело ошибки имеет вид {"error":"..."}
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (doc.TryGetProperty("error", out var err))
                return err.GetString() ?? "Ошибка входа";
        }
        catch { /* не json */ }

        return response.StatusCode == System.Net.HttpStatusCode.Unauthorized
            ? "Неверный email или пароль"
            : $"Ошибка ({(int)response.StatusCode})";
    }

    /// <summary>
    /// Достаёт userId и username из payload JWT БЕЗ проверки подписи
    /// (подпись валидирует сервер на каждом запросе — клиенту нужно лишь прочитать claims).
    /// Шаги: берём среднюю часть токена (header.<b>payload</b>.signature) →
    /// переводим из base64url в обычный base64 → декодируем JSON → читаем стандартные
    /// claim-URI <c>nameidentifier</c> и <c>name</c> (именно так их кладёт серверный JwtService).
    /// Любая ошибка парсинга → (null, null), без исключений наружу.
    /// </summary>
    private static (int? userId, string? username) ParseToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return (null, null);
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return (null, null);

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }

            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = doc.RootElement;

            int? uid = null;
            string? name = null;
            const string idClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            const string nameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

            if (root.TryGetProperty(idClaim, out var idEl) && int.TryParse(idEl.GetString(), out var i))
                uid = i;
            if (root.TryGetProperty(nameClaim, out var nameEl))
                name = nameEl.GetString();

            return (uid, name);
        }
        catch
        {
            return (null, null);
        }
    }

    // SecureStorage предпочтительно; если недоступно (напр. keychain) — Preferences
    private static async Task SaveTokenAsync(string token)
    {
        try { await SecureStorage.Default.SetAsync(TokenKey, token); }
        catch { Preferences.Default.Set(TokenKey, token); }
    }

    private static async Task<string?> LoadTokenAsync()
    {
        try
        {
            var t = await SecureStorage.Default.GetAsync(TokenKey);
            if (!string.IsNullOrWhiteSpace(t)) return t;
        }
        catch { /* упадём на Preferences */ }

        var pref = Preferences.Default.Get<string?>(TokenKey, null);
        return string.IsNullOrWhiteSpace(pref) ? null : pref;
    }
}
