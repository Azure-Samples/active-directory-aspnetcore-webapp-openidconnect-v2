using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Options;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class ConfidentialClientApplicationService : IConfidentialClientApplicationService
    {
        private AzureAdOptions _azureAdOptions;

        public ConfidentialClientApplicationService(IOptions<AzureAdOptions> azureAdOptions)
        {
            _azureAdOptions = azureAdOptions.Value;
        }

        private IConfidentialClientApplication? _confidentialClientApplication;
        private IConfidentialClientApplication ConfidentialClientApplication
        {
            get
            {
                if (_confidentialClientApplication is null)
                {

                    var clientSecretPlaceholderValue = "[Enter here a client secret for your application]";

                    if (!string.IsNullOrWhiteSpace(_azureAdOptions.ClientSecret) &&
                        _azureAdOptions.ClientSecret != clientSecretPlaceholderValue)
                    {
                        _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                            .WithClientSecret(_azureAdOptions.ClientSecret)
                            .WithRedirectUri(_azureAdOptions.RedirectUri)
                            .WithAuthority(new Uri(_azureAdOptions.Authority))
                            .Build();
                    }
                    else if (_azureAdOptions.Certificate is not null)
                    {
                        ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                        certificateLoader.LoadIfNeeded(_azureAdOptions.Certificate);

                        _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                            .WithCertificate(_azureAdOptions.Certificate.Certificate)
                            .WithRedirectUri(_azureAdOptions.RedirectUri)
                            .WithAuthority(new Uri(_azureAdOptions.Authority))
                            .Build();
                    }
                    else
                    {
                        throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
                    }

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
            if (userAccount is not null)
            {
                await ConfidentialClientApplication.RemoveAsync(userAccount);
            }
        }
    }
}
