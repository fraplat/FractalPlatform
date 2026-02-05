using EmbedIO;
using EmbedIO.Actions;
using System.Diagnostics;
using System.Text;

namespace FractalPlatform.MAUI
{
	public class LocalHttpServer
	{
		private WebServer _server;
		private readonly int _port;
		private readonly string _baseUrl;

		public LocalHttpServer(int port = 8080)
		{
			_port = port;
			_baseUrl = $"http://localhost:{_port}/";
		}

		public string BaseUrl => _baseUrl;

		public void Start()
		{
			if (_server != null)
			{
				Debug.WriteLine("[HttpServer] Already running");
				return;
			}

			_server = new WebServer(o => o
					.WithUrlPrefix(_baseUrl)
					.WithMode(HttpListenerMode.EmbedIO))
				.WithModule(new ActionModule("/", HttpVerbs.Any, HandleRequest));

			_server.StateChanged += (s, e) =>
				Debug.WriteLine($"[HttpServer] State: {e.NewState}");

			_server.RunAsync();

			Debug.WriteLine($"[HttpServer] Started on {_baseUrl}");
		}

		public void Stop()
		{
			_server?.Dispose();
			_server = null;
			Debug.WriteLine("[HttpServer] Stopped");
		}

		private async Task HandleRequest(IHttpContext context)
		{
			var path = context.Request.Url.AbsolutePath;
			Debug.WriteLine($"[HttpServer] Request: {path}");

			try
			{
				// HTML страница
				if (path == "/" || path == "/index.html")
				{
					await ServeHtml(context);
					return;
				}

				// Изображения
				if (path.EndsWith(".jpg") || path.EndsWith(".jpeg") ||
					path.EndsWith(".png") || path.EndsWith(".gif"))
				{
					await ServeImage(context, path);
					return;
				}

				// CSS
				if (path.EndsWith(".css"))
				{
					await ServeCss(context, path);
					return;
				}

				// JavaScript
				if (path.EndsWith(".js"))
				{
					await ServeJs(context, path);
					return;
				}

				// 404
				context.Response.StatusCode = 404;
				await context.Response.OutputStream.WriteAsync(
					Encoding.UTF8.GetBytes("Not Found"));
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[HttpServer] Error: {ex.Message}");
				context.Response.StatusCode = 500;
			}
		}

		private async Task ServeHtml(IHttpContext context)
		{
			string html = @"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body { font-family: Arial; padding: 20px; background: #f0f0f0; }
        img { max-width: 100%; border: 3px solid #333; margin: 10px 0; display: block; }
        h1 { color: #333; }
        .info { background: white; padding: 15px; margin: 10px 0; border-radius: 5px; }
    </style>
</head>
<body>
    <h1>Локальний HTTP-сервер працює!</h1>
    
    <div class='info'>
        <h2>Зовнішнє зображення (перехоплено):</h2>
        <img src='https://nstatic.nova.bg/public/pics/mynews/980x551_7d092874163ef9dcd53abdd41058f58d.jpg' />
    </div>
    
    <div class='info'>
        <h2>Локальне зображення:</h2>
        <img src='/images/local.jpg' />
    </div>
    
    <div class='info'>
        <h2>Згенероване зображення:</h2>
        <img src='/images/generated.png' />
    </div>
</body>
</html>";

			context.Response.ContentType = "text/html; charset=utf-8";
			await context.Response.OutputStream.WriteAsync(
				Encoding.UTF8.GetBytes(html));
		}

		private async Task ServeImage(IHttpContext context, string path)
		{
			Debug.WriteLine($"[HttpServer] Serving image: {path}");

			byte[] imageData = null;
			string contentType = "image/jpeg";

			// Приклад 1: Відповідь з кастомним зображенням
			if (path.Contains("generated"))
			{
				// Синій квадрат
				imageData = Convert.FromBase64String(
					"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="
				);
				contentType = "image/png";
			}
			// Приклад 2: Завантаження з ресурсів
			else if (path.Contains("local"))
			{
				try
				{
					using var stream = await FileSystem.OpenAppPackageFileAsync("image.jpg");
					using var memoryStream = new MemoryStream();
					await stream.CopyToAsync(memoryStream);
					imageData = memoryStream.ToArray();
				}
				catch
				{
					// Fallback якщо файл не знайдено
					imageData = Convert.FromBase64String(
						"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg=="
					);
					contentType = "image/png";
				}
			}
			// Приклад 3: Проксі зовнішнього зображення
			else
			{
				try
				{
					using var httpClient = new HttpClient();
					imageData = await httpClient.GetByteArrayAsync(
						"https://via.placeholder.com/400x300/FF0000/FFFFFF?text=Custom"
					);
				}
				catch
				{
					// Червоний пікель якщо помилка
					imageData = Convert.FromBase64String(
						"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg=="
					);
					contentType = "image/png";
				}
			}

			context.Response.ContentType = contentType;
			context.Response.ContentLength64 = imageData.Length;
			await context.Response.OutputStream.WriteAsync(imageData, 0, imageData.Length);
		}

		private async Task ServeCss(IHttpContext context, string path)
		{
			string css = "body { background: #f0f0f0; }";
			context.Response.ContentType = "text/css; charset=utf-8";
			await context.Response.OutputStream.WriteAsync(
				Encoding.UTF8.GetBytes(css));
		}

		private async Task ServeJs(IHttpContext context, string path)
		{
			string js = "console.log('Loaded from local server');";
			context.Response.ContentType = "application/javascript; charset=utf-8";
			await context.Response.OutputStream.WriteAsync(
				Encoding.UTF8.GetBytes(js));
		}
	}
}