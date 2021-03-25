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
        private readonly AdfsOptions _adfsOptions;
        private readonly Func<Dispatcher> _getDispatcher;
        private readonly Func<IntPtr> _getParentWindowHandler;
        private readonly string[] _scopes;

        private IPublicClientApplication _app;

        public AdfsTokenService(AdfsOptions adfsOptions, Func<Dispatcher> getDispatcher, Func<IntPtr> getParentWindowHandler)
        {
            _adfsOptions = adfsOptions;
            _getDispatcher = getDispatcher;
            _getParentWindowHandler = getParentWindowHandler;
            _scopes = new[] { $"{adfsOptions.ResourceUrl}/openid" };
        }

        public void Init()
        {
            if (_app != null)
                return;

            var builder = PublicClientApplicationBuilder.Create(_adfsOptions.ClientId)
                .WithAdfsAuthority(_adfsOptions.Authority, false)
                .WithRedirectUri(_adfsOptions.RedirectUrl);

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
                    .WithCustomWebUi(new CustomWebUi(_getDispatcher(), _adfsOptions.RedirectUrl))
                    .WithParentActivityOrWindow(_getParentWindowHandler())
                    .ExecuteAsync();
            }

            return authResult?.AccessToken;
        }
    }
}