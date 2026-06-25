using Microsoft.Maui.Storage;

namespace itmodd.Services;

// Настройки доступа к API. ВРЕМЕННО: токен вставляется вручную.
public static class ApiConfig
{
    // адрес API. localhost подходит для MacCatalyst/Windows.
    // (для Android-эмулятора будет 10.0.2.2, для реального устройства — IP машины)
    public const string BaseUrl = "http://localhost:5227";

    // ВРЕМЕННО: вставь сюда JWT из POST /login (Swagger на {BaseUrl}/swagger),
    // либо положи токен в Preferences["jwt"]. Когда появится экран логина — уберём.
    public const string DevToken = "";

    // активный токен: сначала смотрим Preferences, потом DevToken
    public static string? Token
    {
        get
        {
            var saved = Preferences.Default.Get<string?>("jwt", null);
            if (!string.IsNullOrWhiteSpace(saved)) return saved;
            return string.IsNullOrWhiteSpace(DevToken) ? null : DevToken;
        }
    }
}
