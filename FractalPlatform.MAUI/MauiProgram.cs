using FractalPlatform.MAUI;

public static class MauiProgram
{
	public static LocalHttpServer LocalServer { get; private set; }

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		// Запускаємо HTTP-сервер
		LocalServer = new LocalHttpServer(8080);
		LocalServer.Start();

		return builder.Build();
	}
}