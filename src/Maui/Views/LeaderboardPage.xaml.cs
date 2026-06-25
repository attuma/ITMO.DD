using itmodd.Services;
using itmodd.ViewModels;

namespace itmodd.Views;

/// <summary>
/// Экран «Рейтинг». Резолвит сервисы из DI-контейнера, а если он почему-то
/// недоступен — создаёт их вручную (фолбэк), чтобы страница не падала.
/// Так же сделаны Timer/AddTask — единый паттерн по проекту.
/// </summary>
public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderboardViewModel _vm;

    public LeaderboardPage()
    {
        InitializeComponent();

        // достаём общие синглтоны (IApiClient/IAuthService) из MAUI DI;
        // ?? new ... — запасной путь, если контейнер ещё не готов
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var auth = services?.GetService<IAuthService>() ?? new AuthService(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) });
        var api = services?.GetService<IApiClient>()
                  ?? new ApiClient(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) }, auth);

        _vm = new LeaderboardViewModel(api, auth);
        BindingContext = _vm;
    }

    /// <summary>Каждый раз при показе экрана подгружаем свежий рейтинг.</summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
    }
}
