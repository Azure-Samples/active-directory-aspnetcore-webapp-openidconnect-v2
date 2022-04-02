using Microsoft.Identity.Client;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public interface IConfidentialClientApplicationService
    {
        public Task<AuthenticationResult> GetAuthenticationResultAsync(IEnumerable<string> scopes, string authorizationCode, string codeVerifier);
        public Task RemoveAccount(string identifier);
    }
}
