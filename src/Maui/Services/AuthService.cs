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

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public string? Token => _token;
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_token);

    public async Task<bool> TryRestoreAsync()
    {
        _token = await LoadTokenAsync();
        return IsAuthenticated;
    }

    public Task<AuthResult> LoginAsync(string email, string password) =>
        AuthenticateAsync("/login", new { email, password });

    public Task<AuthResult> RegisterAsync(string username, string email, string password) =>
        AuthenticateAsync("/register", new { username, email, password });

    public async Task LogoutAsync()
    {
        _token = null;
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

            _token = auth.Token;
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
