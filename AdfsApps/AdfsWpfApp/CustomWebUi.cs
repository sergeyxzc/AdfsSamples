using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Threading;
using Microsoft.Identity.Client.Extensibility;
using Microsoft.Web.WebView2.Wpf;

namespace AdfsWpfApp
{
    public class CustomWebUi : ICustomWebUi
    {
        private readonly Dispatcher _dispatcher;
        private readonly string _redirectUrl;

        public CustomWebUi(Dispatcher dispatcher, string redirectUrl)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _redirectUrl = redirectUrl ?? throw new ArgumentNullException(nameof(redirectUrl));
        }

        public Task<Uri> AcquireAuthorizationCodeAsync(Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Uri>();

            _dispatcher.InvokeAsync(() =>
            {
                var w = new LoginWindow();

                var webView = w.WebView;
                webView.EnsureCoreWebView2Async();

                webView.CoreWebView2InitializationCompleted += (sender, args) =>
                {
                    var str = authorizationUri.ToString();
                    webView.CoreWebView2.Navigate(str);
                };

                webView.NavigationCompleted += (sender, args) =>
                {
                    var wv = sender as WebView2;
                    if (wv == null)
                        return;

                    if (!wv.Source.ToString().StartsWith(_redirectUrl))
                    {
                        return;
                    }

                    tcs.SetResult(wv.Source);
                    w.DialogResult = true;
                    w.Close();

                    var query = HttpUtility.ParseQueryString(wv.Source.Query);
                    if (query.AllKeys.Any(x => x == "code"))
                    {
                        tcs.SetResult(wv.Source);
                        w.DialogResult = true;
                        w.Close();
                        return;
                    }

                    tcs.SetException(new Exception($"An error occurred, error: {query.Get("error")}, error_description: {query.Get("error_description")}"));
                    w.DialogResult = false;
                    w.Close();
                };

                if (w.ShowDialog() != true && !tcs.Task.IsCompleted)
                {
                    tcs.SetException(new Exception("canceled"));
                }
            });

            return tcs.Task;
        }
    }
}