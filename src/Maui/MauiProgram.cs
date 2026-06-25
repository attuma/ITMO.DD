using Microsoft.Extensions.Logging;
using itmodd.Services;

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
