using itmodd.Services;
using itmodd.ViewModels;

namespace itmodd.Views;

public partial class TimerPage : ContentPage
{
    private readonly TimerViewModel _vm;

    public TimerPage()
    {
        InitializeComponent();

        var services = Application.Current?.Handler?.MauiContext?.Services;
        var api = services?.GetService<IApiClient>()
                  ?? new ApiClient(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) },
                                   services?.GetService<IAuthService>() ?? new AuthService(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) }));

        _vm = new TimerViewModel(api);
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
    }
}
