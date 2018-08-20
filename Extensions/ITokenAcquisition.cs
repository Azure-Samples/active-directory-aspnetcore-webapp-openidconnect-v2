using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TodoListService.Extensions
{
    public interface ITokenAcquisition
    {
        /// <summary>
        /// Add, to the MSAL.NET cache, the account of the user for which a bearer token was received when the Web API was called.
        /// An On-behalf-of token is added to the cache, so that it can then be used to acquire another token on On-behalf-of the 
        /// same user in order to call to downstream APIs.
        /// </summary>
        /// <param name="userAccessToken">Access token used to call this Web API</param>
        /// <example>
        /// From the configuration of the Authentication of the ASP.NET Core Web API: 
        /// <code>JwtBearerOptions option;</code>
        /// 
        /// Subscribe to the token validated event:
        /// <code>
        /// options.Events = new JwtBearerEvents();
        /// options.Events.OnTokenValidated = OnTokenValidated;
        /// }
        /// </code>
        /// 
        /// And then in the OnTokenValidated method, call <see cref="AddAccountToCache(JwtSecurityToken)"/>:
        /// <code>
        /// private async Task OnTokenValidated(TokenValidatedContext context)
        /// {
        ///  JwtSecurityToken accessToken = context.SecurityToken as JwtSecurityToken;
        ///  _tokenAcquisition.AddAccountToCache(accessToken);
        /// }
        /// </code>
        /// </example>
        void AddAccountToCacheFromJwt(JwtSecurityToken jwtToken);

        void AddAccountToCacheFromAuthorizationCode(string code);

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user account which claims are provided in the 
        /// <paramref name="user"/> parameter
        /// </summary>
        /// <param name="user">User account described by its claims</param>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <returns>An access token to call on behalf of the user, the downstream API characterized by its scopes</returns>
        Task<string> GetAccessTokenOnBehalfOfUser(ClaimsPrincipal user, string[] scopes);

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user of a given account ID
        /// </summary>
        /// <param name="accountIdentifier">User account identifier for which to acquire a token. 
        /// See <see cref="Microsoft.Identity.Client.AccountId.Identifier"/> for a description on how
        /// to get the account identifier</param>
        /// <param name="scopes">Scopes for the downstream API to call</param>
        Task<string> GetAccessTokenOnBehalfOfUser(string userId, string[] scopes);

        /// <summary>
        /// Access to the MSAL.NET confidential client application (for advanced scenarios)
        /// </summary>
        ConfidentialClientApplication Application { get; }
    }
}