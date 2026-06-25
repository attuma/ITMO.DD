namespace itmodd.Services;

// результат попытки входа/регистрации
public record AuthResult(bool Success, string? Error = null);

public interface IAuthService
{
    // текущий JWT (в памяти); null = не авторизован
    string? Token { get; }
    bool IsAuthenticated { get; }

    // восстановить сессию из хранилища (при старте). true = токен есть
    Task<bool> TryRestoreAsync();

    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string username, string email, string password);
    Task LogoutAsync();
}
