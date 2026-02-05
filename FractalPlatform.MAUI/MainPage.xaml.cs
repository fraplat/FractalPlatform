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
			webView.Source = "http://127.0.0.1:8123/";
		}
	}
}