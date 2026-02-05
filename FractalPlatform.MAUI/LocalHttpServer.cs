using System.Net;
using System.Net.Sockets;
using System.Text;

public class MiniHttpServer
{
	private TcpListener _listener;
	private bool _running;

	public void Start(int port = 8123)
	{
		_listener = new TcpListener(IPAddress.Loopback, port);
		_listener.Start();
		_running = true;

		Task.Run(ListenLoop);
	}

	private async Task ListenLoop()
	{
		while (_running)
		{
			var client = await _listener.AcceptTcpClientAsync();
			_ = Task.Run(() => HandleClient(client));
		}
	}

	private async Task HandleClient(TcpClient client)
	{
		using var stream = client.GetStream();
		using var reader = new StreamReader(stream, Encoding.UTF8);
		using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

		// 1. Читаем headers
		string line;
		string method = "";
		string path = "";
		int contentLength = 0;

		while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
		{
			if (line.StartsWith("GET") || line.StartsWith("POST"))
			{
				var parts = line.Split(' ');
				method = parts[0];
				path = parts[1];
			}
			else if (line.StartsWith("Content-Length", StringComparison.OrdinalIgnoreCase))
			{
				contentLength = int.Parse(line.Split(':')[1].Trim());
			}
		}

		// 2. Читаем body (если POST)
		string body = "";
		if (method == "POST" && contentLength > 0)
		{
			var buffer = new char[contentLength];
			await reader.ReadAsync(buffer, 0, contentLength);
			body = new string(buffer);
		}

		Console.WriteLine($"HTTP {method} {path}");
		Console.WriteLine(body);

		// 3. Ответ
		string responseBody = path switch
		{
			"/submit" => $"Received POST:\n{body}",
			_ => "<html><body><h1>Fractal runtime</h1></body></html>"
		};

		byte[] bytes = Encoding.UTF8.GetBytes(responseBody);

		await writer.WriteAsync(
			"HTTP/1.1 200 OK\r\n" +
			"Content-Type: text/html; charset=utf-8\r\n" +
			$"Content-Length: {bytes.Length}\r\n" +
			"Connection: close\r\n\r\n"
		);

		await stream.WriteAsync(bytes);
		client.Close();
	}

	public void Stop()
	{
		_running = false;
		_listener.Stop();
	}
}
