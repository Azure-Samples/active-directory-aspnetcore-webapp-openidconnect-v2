// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Interface for the token acquisition service (encapsultating MSAL.NET)
    /// </summary>
    public interface ITokenAcquisition
    {
        /// <summary>
        /// In a Web App, adds, to the MSAL.NET cache, the account of the user authenticating to the Web App, when the authorization code is received (after the user
        /// signed-in and consented)
        /// An On-behalf-of token contained in the <see cref="AuthorizationCodeReceivedContext"/> is added to the cache, so that it can then be used to acquire another token on-behalf-of the 
        /// same user in order to call to downstream APIs.
        /// </summary>
        /// <param name="context">The context used when an 'AuthorizationCode' is received over the OpenIdConnect protocol.</param>
        /// <param name="scopes">Scopes to request</param>
        /// <example>
        /// From the configuration of the Authentication of the ASP.NET Core Web API: 
        /// <code>OpenIdConnectOptions options;</code>
        /// 
        /// Subscribe to the authorization code received event:
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
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService&lt;ITokenAcquisition&gt;();
        ///    await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, new string[] { "user.read" });
        /// }
        /// </code>
        /// </example>
        Task AddAccountToCacheFromAuthorizationCodeAsync(AuthorizationCodeReceivedContext context, IEnumerable<string> scopes);

        /// <summary>
        /// Typically used from an ASP.NET Core Web App or Web API controller, this method gets an access token 
        /// for a downstream API on behalf of the user account which claims are provided in the <see cref="HttpContext.User"/>
        /// member of the <paramref name="context"/> parameter
        /// </summary>
        /// <param name="context">HttpContext associated with the Controller or auth operation</param>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <param name="tenantId">Enables to override the tenant/account for the same identity. This is useful in the 
        /// cases where a given account is guest in other tenants, and you want to acquire tokens for a specific tenant</param>
        /// <returns>An access token to call on behalf of the user, the downstream API characterized by its scopes</returns>
        Task<string> GetAccessTokenOnBehalfOfUserAsync(IEnumerable<string> scopes, string tenantId = null);

        /// <summary>
        /// Removes the account associated with context.HttpContext.User from the MSAL.NET cache
        /// </summary>
        /// <param name="context">RedirectContext passed-in to a <see cref="OnRedirectToIdentityProviderForSignOut"/> 
        /// Openidconnect event</param>
        /// <returns></returns>
        Task RemoveAccountAsync(RedirectContext context);

        /// <summary>
        /// Used in Web APIs (which therefore cannot have an interaction with the user). 
        /// Replies to the client through the HttpReponse by sending a 403 (forbidden) and populating wwwAuthenticateHeaders so that
        /// the client can trigger an interaction with the user so that the user consents to more scopes
        /// </summary>
        /// <param name="scopes">Scopes to consent to</param>
        /// <param name="msalSeviceException"><see cref="MsalUiRequiredException"/> triggering the challenge</param>
        void ReplyForbiddenWithWwwAuthenticateHeader(
            IEnumerable<string> scopes,
            MsalUiRequiredException msalSeviceException);
    }
}