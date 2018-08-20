using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TodoListService.Extensions
{
    public class TokenAcquisition : ITokenAcquisition
    {
        /// <summary>
        /// Constructor of the TokenAcquisition service. This requires the Azure AD Options to 
        /// configure the confidential client application
        /// </summary>
        /// <param name="options">Options to configure the application</param>
        public TokenAcquisition(IOptions<AzureAdOptions> options)
        {
            GetOrCreateApplication(options.Value);
        }

        /// <summary>
        /// The goal of this method is, when a user is authenticated, to add the user's account in the MSAL.NET cache
        /// so that this token can then be used to acquire a token on On-behalf-of the user befor call to downstream APIs.
        /// </summary>
        /// <param name="userAccessToken">Access token used to call this Web API</param>
        /// <example>
        /// From configuration of the Authentication of the ASP.NET Core Web API 
        /// <code>JwtBearerOptions option;</code>
        /// 
        /// Subscribe to the token validated event
        /// <code>
        /// options.Events = new JwtBearerEvents();
        /// options.Events.OnTokenValidated = OnTokenValidated;
        /// }
        /// </code>
        /// 
        /// And then in the OnTokenValidated method:
        /// <code>
        /// private async Task OnTokenValidated(TokenValidatedContext context)
        /// {
        ///  JwtSecurityToken accessToken = context.SecurityToken as JwtSecurityToken;
        ///  _tokenAcquisition.AddAccountToCache(accessToken);
        /// }
        /// </code>
        /// </example>
        public void AddAccountToCacheFromJwt(JwtSecurityToken jwtToken)
        {
            string userAccessTokenForThisApi = jwtToken.RawData;
            string[] scopes = new string[] { "user.read" };
            try
            {
                UserAssertion userAssertion = new UserAssertion(userAccessTokenForThisApi, "urn:ietf:params:oauth:grant-type:jwt-bearer");

                // .Result to make sure that the cache is filled-in before the controller tries to get access tokens
                AuthenticationResult result = Application.AcquireTokenOnBehalfOfAsync(scopes, userAssertion).Result;
                string acessTokenForGraphOBOUser = result.AccessToken;
            }
            catch (MsalException ex)
            {
                string message = ex.Message;
                throw;
            }
        }

        public void AddAccountToCacheFromAuthorizationCode(string authorizationCode)
        {
            string[] scopes = new string[] { "user.read" };
            try
            {
                // .Result to make sure that the cache is filled-in before the controller tries to get access tokens
                AuthenticationResult result = Application.AcquireTokenByAuthorizationCodeAsync(authorizationCode, scopes).Result;
                string acessTokenForGraphOBOUser = result.AccessToken;
            }
            catch (MsalException ex)
            {
                string message = ex.Message;
                throw;
            }

        }


        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user which evidence are provided by the
        /// <paramref name="user"/> parameter
        /// </summary>
        /// <param name="user">Account described by its claims</param>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <returns>An access token to call the downstream API characterized by its scopes, on behalf of the user</returns>
        public async Task<string> GetAccessTokenOnBehalfOfUser(ClaimsPrincipal user, string[] scopes)
        {
            string userObjectId = user.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            string tenantId = user.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
            if (string.IsNullOrWhiteSpace(userObjectId))
            {
                // TODO: find a better typed exception
                throw new Exception("Missing claim 'http://schemas.microsoft.com/identity/claims/objectidentifier'");
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new Exception("Missing claim 'http://schemas.microsoft.com/identity/claims/tenantid'");
            }
            string userId = userObjectId + "." + tenantId;
            return await GetAccessTokenOnBehalfOfUser(userId, scopes);
        }

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user which account ID is passed as an argument
        /// </summary>
        /// <param name="accountIdentifier">User account identifier for which to acquire a token. 
        /// See <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></param>
        /// <param name="scopes">Scopes for the downstream API to call</param>
        public async Task<string> GetAccessTokenOnBehalfOfUser(string accountIdentifier, string[] scopes)
        {
            var accounts = (await Application.GetAccountsAsync());

            string accessToken = null;
            try
            {
                AuthenticationResult result = null;
                IAccount account = await Application.GetAccountAsync(accountIdentifier);
                result = await Application.AcquireTokenSilentAsync(scopes, account);
                accessToken = result.AccessToken;
            }
            catch (MsalException ex)
            {
                // TODO process the exception see if this is retryable etc ...
                throw;
            }

            return accessToken;
        }

        /// <summary>
        /// Access to the MSAL.NET confidential client application (for advanced scenarios)
        /// </summary>
        public ConfidentialClientApplication Application { get; private set; }

        // Todo provide a better cache
        static TokenCache userTokenCache = new TokenCache();

        private void GetOrCreateApplication(AzureAdOptions options)
        {
            if (Application == null)
            {

                // This is a confidential client applicaiton, and therefore it shares with Azure AD client credentials (a client secret
                // like here, but could also be a certificate)
                ClientCredential clientCredential = new ClientCredential(options.ClientSecret);

                // MSAL requests tokens from the Azure AD v2.0 endpoint
                string authority = $"{options.Instance}{options.TenantId}/v2.0/";

                Application = new ConfidentialClientApplication(options.ClientId, authority, options.RedirectUri,
                                                                clientCredential, userTokenCache, appTokenCache: null);
            }
        }

    }
}
