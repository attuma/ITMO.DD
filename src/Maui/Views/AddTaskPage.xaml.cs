using itmodd.Services;
using itmodd.ViewModels;

namespace itmodd.Views;

public partial class AddTaskPage : ContentPage
{
    private readonly AddTaskViewModel _vm;
    private readonly Func<Task>? _onSaved;

    public AddTaskPage(Func<Task>? onSaved = null, DateTime? initialDate = null)
    {
        InitializeComponent();
        _onSaved = onSaved;

        var services = Application.Current?.Handler?.MauiContext?.Services;
        var api = services?.GetService<IApiClient>()
                  ?? new ApiClient(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) },
                                   services?.GetService<IAuthService>() ?? new AuthService(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) }));

        _vm = new AddTaskViewModel(api);

        // если в календаре был выбран день — стартуем форму с его датой
        if (initialDate is { } date)
            _vm.DeadlineDate = date;

        _vm.Saved += OnSaved;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
    }

    private async void OnSaved()
    {
        if (_onSaved is not null)
            await _onSaved();
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
