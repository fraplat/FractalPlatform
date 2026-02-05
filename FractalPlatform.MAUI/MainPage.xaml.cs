namespace FractalPlatform.MAUI
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await Task.Delay(300); // да, это нормально на Android

			webView.Source = "http://127.0.0.1:8123/";
		}
	}
}