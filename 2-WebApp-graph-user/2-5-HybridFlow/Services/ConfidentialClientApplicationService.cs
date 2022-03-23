using System.Security.Claims;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class ConfidentialClientApplicationService : IConfidentialClientApplicationService
    {
        private static AuthenticationConfig _authenticationConfig;
        private static AuthenticationConfig AuthenticationConfig
        {
            get
            {
                if (_authenticationConfig == null)
                {
                    _authenticationConfig = AuthenticationConfig.ReadFromJsonFile("appsettings.json");
                }

                return _authenticationConfig;
            }
        }

        private static IConfidentialClientApplication? _confidentialClientApplication;
        private static IConfidentialClientApplication ConfidentialClientApplication
        {
            get
            {
                if (_confidentialClientApplication == null)
                {
                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(AuthenticationConfig.AzureAd.ClientId)
                        .WithClientSecret(AuthenticationConfig.AzureAd.ClientSecret)
                        .WithRedirectUri(AuthenticationConfig.RedirectUri)
                        .WithAuthority(new Uri(AuthenticationConfig.AzureAd.Authority))
                        .Build();

                    _confidentialClientApplication.AddInMemoryTokenCache();
                }

                return _confidentialClientApplication;
            }
        }

        private string[] _applicationScopes;
        private string[] ApplicationScopes
        {
            get
            {
                if (_applicationScopes == null)
                {
                    _applicationScopes = AuthenticationConfig.DownstreamApi.Scopes.Split(' ');
                }

                return _applicationScopes;
            }
        }

        public async Task<AuthenticationResult> GetAuthenticationResultAsync(string code, string codeVerifier)
        {
            return await ConfidentialClientApplication
                .AcquireTokenByAuthorizationCode(ApplicationScopes, code)
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
