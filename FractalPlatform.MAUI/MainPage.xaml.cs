namespace FractalPlatform.MAUI
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
			LoadPage();
		}

		private void LoadPage()
		{
			// Просто завантажуємо localhost
			webView.Source = MauiProgram.LocalServer.BaseUrl;
		}
	}
}