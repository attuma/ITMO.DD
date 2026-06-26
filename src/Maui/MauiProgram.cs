using Microsoft.Extensions.Logging;
using itmodd.Services;
using itmodd.ViewModels;
using itmodd.Views;

namespace itmodd;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// HttpClient для обращения к API
		builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) });

		// авторизация (логин/регистрация + хранение токена)
		builder.Services.AddSingleton<IAuthService, AuthService>();

		// клиент API для создания задач и работы с предметами
		builder.Services.AddSingleton<IApiClient, ApiClient>();

		// экраны и VM для групп
		builder.Services.AddTransient<GroupsViewModel>();
		builder.Services.AddTransient<GroupsPage>();
		builder.Services.AddTransient<GroupDetailViewModel>();
		builder.Services.AddTransient<GroupDetailPage>();

		// источник данных календаря.
		// ApiCalendarDataService — живые данные (нужен JWT в ApiConfig.Token и запущенный API).
		// Для работы без сервера верни StubCalendarDataService.
		builder.Services.AddSingleton<ICalendarDataService, ApiCalendarDataService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
