using FractalPlatform.MAUI;

public static class MauiProgram
{
	public static MiniHttpServer Server;

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
		Server = new MiniHttpServer();
		Server.Start(8123);

		return builder.Build();
	}
}