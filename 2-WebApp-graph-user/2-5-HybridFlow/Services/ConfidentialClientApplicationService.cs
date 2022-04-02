using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Options;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class ConfidentialClientApplicationService : IConfidentialClientApplicationService
    {
        private static AzureAdOptions _azureAdOptions;

        public ConfidentialClientApplicationService(IOptions<AzureAdOptions> azureAdOptions)
        {
            _azureAdOptions = azureAdOptions.Value;
        }

        private static IConfidentialClientApplication? _confidentialClientApplication;
        private static IConfidentialClientApplication ConfidentialClientApplication
        {
            get
            {
                if (_confidentialClientApplication == null)
                {
                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                        .WithClientSecret(_azureAdOptions.ClientSecret)
                        .WithRedirectUri(_azureAdOptions.RedirectUri)
                        .WithAuthority(new Uri(_azureAdOptions.Authority))
                        .Build();

                    _confidentialClientApplication.AddInMemoryTokenCache();
                }

                return _confidentialClientApplication;
            }
        }

        public async Task<AuthenticationResult> GetAuthenticationResultAsync(IEnumerable<string> scopes, string authorizationCode, string codeVerifier)
        {
            return await ConfidentialClientApplication
                .AcquireTokenByAuthorizationCode(scopes, authorizationCode)
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
