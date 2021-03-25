using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;

namespace AdfsWpfApp
{
    public class AdfsTokenService
    {
        private readonly string _adfsAuthorityUrl;
        private readonly string _redirectUrl;
        private readonly string _clientId;
        private readonly Func<IntPtr> _getParentWindowHandler;
        private readonly string[] _scopes = new[] { "openid" };

        private IPublicClientApplication _app;

        public AdfsTokenService(string adfsAuthorityUrl, string redirectUrl, string clientId, Func<IntPtr> getParentWindowHandler)
        {
            _adfsAuthorityUrl = adfsAuthorityUrl;
            _redirectUrl = redirectUrl;
            _clientId = clientId;
            _getParentWindowHandler = getParentWindowHandler;
        }

        public void Init()
        {
            if (_app != null)
                return;

            var builder = PublicClientApplicationBuilder.Create(_clientId)
                .WithAdfsAuthority(_adfsAuthorityUrl, false)
                .WithRedirectUri(_redirectUrl);

            _app = builder.Build();
        }

        public async Task<string> GetTokenAsync()
        {
            AuthenticationResult authResult;

            try
            {
                var accounts = await _app.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                authResult = await _app.AcquireTokenSilent(_scopes, firstAccount).ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                authResult = await _app.AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.NoPrompt)
                    .WithCustomWebUi(new CustomWebUi(Dispatcher.CurrentDispatcher, _redirectUrl))
                    .WithParentActivityOrWindow(_getParentWindowHandler())
                    .ExecuteAsync();
            }

            return authResult?.AccessToken;
        }
    }
}