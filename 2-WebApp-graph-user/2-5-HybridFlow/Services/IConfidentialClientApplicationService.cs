using Microsoft.Identity.Client;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public interface IConfidentialClientApplicationService
    {
        public Task<AuthenticationResult> GetAuthenticationResultAsync(string[] scopes, string code, string codeVerifier);
        public Task RemoveAccount(string identifier);
    }
}
