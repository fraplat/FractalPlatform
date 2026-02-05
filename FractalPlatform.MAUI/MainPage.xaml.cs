using System.Net;

namespace FractalPlatform.MAUI;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		LoadCustomHtml();
	}

	private void LoadCustomHtml()
	{
		string customHtml = @"
<!DOCTYPE html>
<html>
<body>
    <h1>Форма</h1>
    <form action='myapp://submit' method='get'>
		<img src='https://www.example.com/image.jpg' alt='Example Image' width='200' />
        <input type='text' name='firstName' placeholder='Ім''я' />
        <input type='text' name='lastName' placeholder='Прізвище' />
        <input type='email' name='email' placeholder='Email' />
        <button type='submit'>Відправити</button>
    </form>
</body>
</html>";

		webView.Navigating += WebView_Navigating;

		webView.Navigated += async (s, e) =>
		{
			await webView.EvaluateJavaScriptAsync(@"
        document.addEventListener('submit', function(e) {
            e.preventDefault();
            const data = {};
            new FormData(e.target).forEach((v, k) => data[k] = v);
            window.location.href = 'myapp://submit?data=' +
                encodeURIComponent(JSON.stringify(data));
        });
    ");
		};

		UpdateHtml(customHtml);
	}

	private void UpdateHtml(string html)
	{
		var htmlSource = new HtmlWebViewSource
		{
			Html = html
		};

		webView.Source = htmlSource;
	}

	private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
	{
		if (e.Url.StartsWith("myapp://submit"))
		{
			e.Cancel = true;

			var uri = new Uri(e.Url);
			var json = Uri.UnescapeDataString(
				uri.Query.Replace("?data=", "")
			);

			UpdateHtml("<b>great</b>");
		}
	}
}