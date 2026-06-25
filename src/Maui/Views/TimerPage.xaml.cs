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
        StartShimmer();
        await _vm.InitAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.AbortAnimation("shimmer");
    }

    // плавно гоняем точки градиента по диагонали (sin -> бесшовный цикл)
    private void StartShimmer()
    {
        var anim = new Animation(v =>
        {
            double x = (Math.Sin(v * 2 * Math.PI) + 1) / 2; // 0..1..0 без рывка
            TimerGradient.StartPoint = new Point(x, 0);
            TimerGradient.EndPoint = new Point(1 - x, 1);
        }, 0, 1);

        anim.Commit(this, "shimmer", length: 4000, easing: Easing.Linear, repeat: () => true);
    }
}
