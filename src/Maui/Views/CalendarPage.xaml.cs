using itmodd.Services;
using itmodd.ViewModels;

namespace itmodd.Views;

public partial class CalendarPage : ContentPage
{
    public CalendarPage()
    {
        InitializeComponent();

        // берём сервис из DI; если контейнер недоступен — безопасный фолбэк на заглушку
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var data = services?.GetService<ICalendarDataService>() ?? new StubCalendarDataService();

        BindingContext = new CalendarViewModel(data);
    }

    // выход: чистим токен и возвращаемся на экран входа
    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var auth = services?.GetService<IAuthService>();
        if (auth is not null)
            await auth.LogoutAsync();

        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new LoginPage();
    }
}
