using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

namespace Microsoft.AspNetCore.Authentication
{
    public interface ITokenAcquisition
    {
        /// <summary>
        /// Add, to the MSAL.NET cache, the account of the user for which a bearer token was received when the Web API was called.
        /// An On-behalf-of token is added to the cache, so that it can then be used to acquire another token on-behalf-of the 
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
        void AddAccountToCacheFromJwt(JwtSecurityToken jwtToken, IEnumerable<string> scopes);

        /// <summary>
        /// Add, to the MSAL.NET cache, the account of the user for which an authorization code was received when the Web API was called.
        /// An On-behalf-of token contained in the <see cref="AuthorizationCodeReceivedContext"/> is added to the cache, so that it can then be used to acquire another token on-behalf-of the 
        /// same user in order to call to downstream APIs.
        /// </summary>
        /// <param name="context">The context used when an 'AuthorizationCode' is received over the OpenIdConnect protocol.</param>
        /// <example>
        /// From the configuration of the Authentication of the ASP.NET Core Web API: 
        /// <code>OpenIdConnectOptions options;</code>
        /// 
        /// Subscribe to the authorization code recieved event:
        /// <code>
        ///  options.Events = new OpenIdConnectEvents();
        ///  options.Events.OnAuthorizationCodeReceived = OnAuthorizationCodeReceived;
        /// }
        /// </code>
        /// 
        /// And then in the OnAuthorizationCodeRecieved method, call <see cref="AddAccountToCacheFromAuthorizationCode"/>:
        /// <code>
        /// private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        /// {
        ///    await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, new string[] { "user.read" });
        /// }
        /// </code>
        /// </example>
        Task AddAccountToCacheFromAuthorizationCode(AuthorizationCodeReceivedContext context, IEnumerable<string> scopes);

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user account which claims are provided in the <see cref="HttpContext.User"/>
        /// member of the <paramref name="context"/> parameter
        /// </summary>
        /// <param name="context">HttpContext associated with the Controller or auth operation</param>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <returns>An access token to call on behalf of the user, the downstream API characterized by its scopes</returns>
        Task<string> GetAccessTokenOnBehalfOfUser(HttpContext context, string[] scopes);

        /// <summary>
        /// Access to the MSAL.NET confidential client application (for advanced scenarios)
        /// </summary>
        ConfidentialClientApplication Application { get; }
    }
}