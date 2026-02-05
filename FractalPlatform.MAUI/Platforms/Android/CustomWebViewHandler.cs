using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System.Text;

#if ANDROID
using Android.Webkit;
using AWebView = Android.Webkit.WebView;
#endif

namespace FractalPlatform.MAUI
{
	public class CustomWebViewHandler : WebViewHandler
	{
		protected override void ConnectHandler(
#if ANDROID
			AWebView platformView
#elif IOS
            WebKit.WKWebView platformView
#else
            object platformView
#endif
		)
		{
			base.ConnectHandler(platformView);

#if ANDROID
			platformView.Settings.JavaScriptEnabled = true;
			platformView.Settings.AllowFileAccess = true;

			platformView.SetWebViewClient(new CustomWebViewClient(this));
#endif
		}
	}

#if ANDROID
	public class CustomWebViewClient : WebViewClient
	{
		private CustomWebViewHandler _handler;

		public CustomWebViewClient(CustomWebViewHandler handler)
		{
			_handler = handler;
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
#endif
}