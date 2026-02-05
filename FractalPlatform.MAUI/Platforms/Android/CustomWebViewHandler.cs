using Microsoft.Maui.Handlers;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace FractalPlatform.MAUI
{
	public class CustomWebViewHandler : WebViewHandler
	{
		protected override void ConnectHandler(AWebView platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Settings.JavaScriptEnabled = true;
			platformView.Settings.AllowFileAccess = true;

			platformView.Post(() =>
			{
				platformView.SetWebViewClient(new CustomWebViewClient(this));
			});
		}
	}

	public class CustomWebViewClient : WebViewClient
	{
		private CustomWebViewHandler _handler;

		public CustomWebViewClient(CustomWebViewHandler handler)
		{
			_handler = handler;
		}

		public override void OnPageFinished(AWebView view, string url)
		{
			base.OnPageFinished(view, url);
			System.Diagnostics.Debug.WriteLine($"Page finished loading: {url}");
		}

		public override WebResourceResponse ShouldInterceptRequest(
			AWebView view, IWebResourceRequest request)
		{
			var url = request.Url.ToString();
			System.Diagnostics.Debug.WriteLine($"Request: {url}");

			// Ваша логіка перехоплення

			return base.ShouldInterceptRequest(view, request);
		}
	}
}