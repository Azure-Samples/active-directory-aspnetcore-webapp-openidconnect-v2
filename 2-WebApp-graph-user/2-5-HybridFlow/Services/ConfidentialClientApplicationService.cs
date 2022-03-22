using System.Security.Claims;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class ConfidentialClientApplicationService : IConfidentialClientApplicationService
    {
        private static IConfidentialClientApplication? _confidentialClientApplication;
        private static IConfidentialClientApplication ConfidentialClientApplication
        {
            get
            {
                if (_confidentialClientApplication == null)
                {
                    var config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(config.AzureAd.ClientId)
                        .WithClientSecret(config.AzureAd.ClientSecret)
                        .WithRedirectUri(config.RedirectUri)
                        .WithAuthority(new Uri(config.AzureAd.Authority))
                        .Build();

                    _confidentialClientApplication.AddInMemoryTokenCache();
                }

                return _confidentialClientApplication;
            }
        }

        private string[] _scopes;
        private string[] Scopes
        {
            get
            {
                if (_scopes == null)
                {
                    var config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");
                    _scopes = config.DownstreamApi.Scopes.Split(' ');
                }

                return _scopes;
            }
        }

        public async Task<AuthenticationResult> GetAuthenticationResultAsync(string code, string codeVerifier)
        {
            return await ConfidentialClientApplication
                .AcquireTokenByAuthorizationCode(Scopes, code)
                .WithPkceCodeVerifier(codeVerifier)
                .WithSpaAuthorizationCode(true)
                .ExecuteAsync();
        }

        public async Task RemoveAccount(string identifier)
        {
            var userAccount = await ConfidentialClientApplication.GetAccountAsync(identifier);
            if (userAccount != null)
            {
                await ConfidentialClientApplication.RemoveAsync(userAccount);
            }
        }
    }
}
