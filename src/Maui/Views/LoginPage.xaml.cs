using itmodd.Services;
using itmodd.ViewModels;

namespace itmodd.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage()
    {
        InitializeComponent();

        var services = Application.Current?.Handler?.MauiContext?.Services;
        var auth = services?.GetService<IAuthService>() ?? new AuthService(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) });

        _vm = new LoginViewModel(auth);
        _vm.Authenticated += GoToApp;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // тихий вход, если токен уже сохранён
        await _vm.TryAutoLoginAsync();
    }

    // после входа — заменяем корень окна на основное приложение (AppShell)
    private void GoToApp()
    {
        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new AppShell();
    }
}
