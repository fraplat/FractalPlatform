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
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            background-color: #f0f0f0;
        }
        h1 {
            color: #333;
        }
    </style>
</head>
<body>
    <h1>Привіт з кастомного HTML!</h1>
    <p>Це HTML завантажений з C# коду.</p>
    <button onclick='alert(""Кнопка натиснута!"")'>Натисни мене</button>
</body>
</html>";

		var htmlSource = new HtmlWebViewSource
		{
			Html = customHtml
		};

		webView.Source = htmlSource;
	}

	private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
	{
		if (e.Url.StartsWith("myapp://submit"))
		{
			e.Cancel = true;

			var uri = new Uri(e.Url);
			var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
			string jsonData = System.Web.HttpUtility.UrlDecode(query["data"]);

			// Десеріалізуємо JSON з усіма полями форми
			var formData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);

			// Тепер у formData всі поля форми
			foreach (var field in formData)
			{
				Console.WriteLine($"{field.Key} = {field.Value}");
			}

			string message = string.Join("\n", formData.Select(kv => $"{kv.Key}: {kv.Value}"));
			DisplayAlert("Form Submitted", message, "OK");
		}
	}
}