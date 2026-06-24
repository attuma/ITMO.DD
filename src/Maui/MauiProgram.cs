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

		// источник данных календаря: пока заглушка, позже подменим на API-реализацию
		builder.Services.AddSingleton<ICalendarDataService, StubCalendarDataService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
