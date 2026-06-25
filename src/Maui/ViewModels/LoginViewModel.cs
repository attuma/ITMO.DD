using itmodd.Services;

namespace itmodd.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy);
        ToggleModeCommand = new Command(() => IsRegisterMode = !IsRegisterMode);
    }

    // успешный вход/регистрация — страница переключит корень на AppShell
    public event Action? Authenticated;

    private string _email = string.Empty;
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

    private string _password = string.Empty;
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

    private string _username = string.Empty;
    public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }

    private bool _isRegisterMode;
    public bool IsRegisterMode
    {
        get => _isRegisterMode;
        set
        {
            _isRegisterMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLoginMode));
            OnPropertyChanged(nameof(SubmitText));
            OnPropertyChanged(nameof(ToggleText));
            ErrorMessage = string.Empty;
        }
    }
    public bool IsLoginMode => !_isRegisterMode;

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); ((Command)SubmitCommand).ChangeCanExecute(); }
    }

    // подписи кнопок зависят от режима
    public string SubmitText => IsRegisterMode ? "Зарегистрироваться" : "Войти";
    public string ToggleText => IsRegisterMode ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Зарегистрироваться";

    public Command SubmitCommand { get; }
    public Command ToggleModeCommand { get; }

    // тихий вход при старте: если токен сохранён — сразу в приложение
    public async Task<bool> TryAutoLoginAsync()
    {
        if (await _auth.TryRestoreAsync())
        {
            Authenticated?.Invoke();
            return true;
        }
        return false;
    }

    private async Task SubmitAsync()
    {
        if (IsBusy) return;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
            (IsRegisterMode && string.IsNullOrWhiteSpace(Username)))
        {
            ErrorMessage = "Заполни все поля";
            return;
        }

        IsBusy = true;
        try
        {
            var result = IsRegisterMode
                ? await _auth.RegisterAsync(Username.Trim(), Email.Trim(), Password)
                : await _auth.LoginAsync(Email.Trim(), Password);

            if (result.Success)
                Authenticated?.Invoke();
            else
                ErrorMessage = result.Error ?? "Не удалось войти";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
