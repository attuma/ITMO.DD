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
}
