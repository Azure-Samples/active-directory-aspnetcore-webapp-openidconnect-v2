/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;
using Microsoft.Identity.Web.Client.TokenCacheProviders;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.Client
{
    /// <summary>
    /// Token acquisition service
    /// </summary>
    public class TokenAcquisition : ITokenAcquisition
    {
        private readonly AzureADOptions azureAdOptions;
        private ConfidentialClientApplicationOptions _applicationOptions;

        private readonly IMSALAppTokenCacheProvider AppTokenCacheProvider;
        private readonly IMSALUserTokenCacheProvider UserTokenCacheProvider;

        /// <summary>
        /// Constructor of the TokenAcquisition service. This requires the Azure AD Options to
        /// configure the confidential client application and a token cache provider.
        /// This constructor is called by ASP.NET Core dependency injection
        /// </summary>
        /// <param name="appTokenCacheProvider">The App token cache provider</param>
        /// <param name="userTokenCacheProvider">The User token cache provider</param>
        /// <param name="configuration"></param>
        public TokenAcquisition(IConfiguration configuration, IMSALAppTokenCacheProvider appTokenCacheProvider, IMSALUserTokenCacheProvider userTokenCacheProvider)
        {
            azureAdOptions = new AzureADOptions();
            configuration.Bind("AzureAD", azureAdOptions);

            _applicationOptions = new ConfidentialClientApplicationOptions();
            configuration.Bind("AzureAD", _applicationOptions);

            this.AppTokenCacheProvider = appTokenCacheProvider;
            this.UserTokenCacheProvider = userTokenCacheProvider;
        }

        /// <summary>
        /// Scopes which are already requested by MSAL.NET. they should not be re-requested;
        /// </summary>
        private readonly string[] scopesRequestedByMsalNet = new string[]
        {
            OidcConstants.ScopeOpenId,
            OidcConstants.ScopeProfile,
            OidcConstants.ScopeOfflineAccess
        };

        /// <summary>
        /// In a Web App, adds, to the MSAL.NET cache, the account of the user authenticating to the Web App, when the authorization code is received (after the user
        /// signed-in and consented)
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
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
        ///    await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, new string[] { "user.read" });
        /// }
        /// </code>
        /// </example>
        public async Task AddAccountToCacheFromAuthorizationCode(AuthorizationCodeReceivedContext context, IEnumerable<string> scopes)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (scopes == null)
                throw new ArgumentNullException(nameof(scopes));

            try
            {
                // As AcquireTokenByAuthorizationCodeAsync is asynchronous we want to tell ASP.NET core that we are handing the code
                // even if it's not done yet, so that it does not concurrently call the Token endpoint.
                context.HandleCodeRedemption();

                var application = BuildConfidentialClientApplication(context.HttpContext, context.Principal);

                // Do not share the access token with ASP.NET Core otherwise ASP.NET will cache it and will not send the OAuth 2.0 request in
                // case a further call to AcquireTokenByAuthorizationCodeAsync in the future for incremental consent (getting a code requesting more scopes)
                // Share the ID Token
                var result = await application.AcquireTokenByAuthorizationCode(scopes.Except(scopesRequestedByMsalNet), context.ProtocolMessage.Code)
                                              .ExecuteAsync();
                context.HandleCodeRedemption(null, result.IdToken);
            }
            catch (MsalException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

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
        public async Task<string> GetAccessTokenOnBehalfOfUser(HttpContext context, IEnumerable<string> scopes, string tenant = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (scopes == null)
                throw new ArgumentNullException(nameof(scopes));

            // Use MSAL to get the right token to call the API
            var application = BuildConfidentialClientApplication(context, context.User);
            return await GetAccessTokenOnBehalfOfUser(application, context.User, scopes, tenant);
        }

        /// <summary>
        /// In a Web API, adds to the MSAL.NET cache, the account of the user for which a bearer token was received when the Web API was called.
        /// An access token and a refresh token are added to the cache, so that they can then be used to acquire another token on-behalf-of the
        /// same user in order to call to downstream APIs.
        /// </summary>
        /// <param name="tokenValidationContext">Token validation context passed to the handler of the OnTokenValidated event
        /// for the JwtBearer middleware</param>
        /// <param name="scopes">[Optional] scopes to pre-request for a downstream API</param>
        /// <example>
        /// From the configuration of the Authentication of the ASP.NET Core Web API (for example in the Startup.cs file)
        /// <code>JwtBearerOptions option;</code>
        ///
        /// Subscribe to the token validated event:
        /// <code>
        /// options.Events = new JwtBearerEvents();
        /// options.Events.OnTokenValidated = async context =>
        /// {
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
        ///   tokenAcquisition.AddAccountToCacheFromJwt(context);
        /// };
        /// </code>
        /// </example>
        public void AddAccountToCacheFromJwt(Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext tokenValidatedContext,
            IEnumerable<string> scopes)
        {
            if (tokenValidatedContext == null)
                throw new ArgumentNullException(nameof(tokenValidatedContext));

            AddAccountToCacheFromJwt(scopes,
                                     tokenValidatedContext.SecurityToken as JwtSecurityToken,
                                     tokenValidatedContext.Principal,
                                     tokenValidatedContext.HttpContext);
        }

        /// <summary>
        /// [not recommended] In a Web App, adds, to the MSAL.NET cache, the account of the user authenticating to the Web App.
        /// An On-behalf-of token is added to the cache, so that it can then be used to acquire another token on-behalf-of the
        /// same user in order for the Web App to call a Web APIs.
        /// </summary>
        /// <param name="tokenValidationContext">Token validation context passed to the handler of the OnTokenValidated event
        /// for the OpenIdConnect middleware</param>
        /// <param name="scopes">[Optional] scopes to pre-request for a downstream API</param>
        /// <remarks>In a Web App, it's preferable to not request an access token, but only a code, and use the <see cref="AddAccountToCacheFromAuthorizationCode"/></remarks>
        /// <example>
        /// From the configuration of the Authentication of the ASP.NET Core Web API:
        /// <code>OpenIdConnectOptions options;</code>
        ///
        /// Subscribe to the token validated event:
        /// <code>
        ///  options.Events.OnAuthorizationCodeReceived = OnTokenValidated;
        /// </code>
        ///
        /// And then in the OnTokenValidated method, call <see cref="AddAccountToCacheFromJwt(OpenIdConnect.TokenValidatedContext)"/>:
        /// <code>
        /// private async Task OnTokenValidated(TokenValidatedContext context)
        /// {
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
        ///  _tokenAcquisition.AddAccountToCache(tokenValidationContext);
        /// }
        /// </code>
        /// </example>
        public void AddAccountToCacheFromJwt(AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext tokenValidatedContext, IEnumerable<string> scopes = null)
        {
            if (tokenValidatedContext == null)
                throw new ArgumentNullException(nameof(tokenValidatedContext));

            AddAccountToCacheFromJwt(scopes,
                                           tokenValidatedContext.SecurityToken,
                                           tokenValidatedContext.Principal,
                                           tokenValidatedContext.HttpContext);
        }

        /// <summary>
        /// Removes the account associated with context.HttpContext.User from the MSAL.NET cache
        /// </summary>
        /// <param name="context">RedirectContext passed-in to a <see cref="OnRedirectToIdentityProviderForSignOut"/>
        /// Openidconnect event</param>
        /// <returns></returns>
        public async Task RemoveAccount(RedirectContext context)
        {
            ClaimsPrincipal user = context.HttpContext.User;
            IConfidentialClientApplication app = BuildConfidentialClientApplication(context.HttpContext, user);
            IAccount account = await app.GetAccountAsync(context.HttpContext.User.GetMsalAccountId());

            // Workaround for the guest account
            if (account == null)
            {
                var accounts = await app.GetAccountsAsync();
                account = accounts.FirstOrDefault(a => a.Username == user.GetLoginHint());
            }

 //           this.AppTokenCacheProvider?.Clear();
            this.UserTokenCacheProvider?.Clear();

            await app.RemoveAsync(account);
        }

        /// <summary>
        /// Creates an MSAL Confidential client application
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="authenticationProperties"></param>
        /// <param name="signInScheme"></param>
        /// <returns></returns>
        private IConfidentialClientApplication BuildConfidentialClientApplication(HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            var request = httpContext.Request;
            string currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, azureAdOptions.CallbackPath ?? string.Empty);
            string authority = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/";

            var app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(_applicationOptions)
               .WithRedirectUri(currentUri)
               .WithAuthority(authority)
               .Build();

            // Initialize token cache providers
            if (this.AppTokenCacheProvider != null)
            {
                this.AppTokenCacheProvider.Initialize(app.AppTokenCache, httpContext);
            }

            if (this.UserTokenCacheProvider != null)
            {
                this.UserTokenCacheProvider.Initialize(app.UserTokenCache, httpContext, claimsPrincipal);
            }

            return app;
        }

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user described by its claimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal for the user on behalf of whom to get a token
        /// <param name="scopes">Scopes for the downstream API to call</param>
        private async Task<string> GetAccessTokenOnBehalfOfUser(IConfidentialClientApplication application, ClaimsPrincipal claimsPrincipal, IEnumerable<string> scopes, string tenant)
        {
            string accountIdentifier = claimsPrincipal.GetMsalAccountId();
            string loginHint = claimsPrincipal.GetLoginHint();
            return await GetAccessTokenOnBehalfOfUser(application, accountIdentifier, scopes, loginHint, tenant);
        }

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user which account ID is passed as an argument
        /// </summary>
        /// <param name="accountIdentifier">User account identifier for which to acquire a token.
        /// See <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></param>
        /// <param name="scopes">Scopes for the downstream API to call</param>
        private async Task<string> GetAccessTokenOnBehalfOfUser(IConfidentialClientApplication application, string accountIdentifier, IEnumerable<string> scopes, string loginHint, string tenant)
        {
            if (accountIdentifier == null)
                throw new ArgumentNullException(nameof(accountIdentifier));

            if (scopes == null)
                throw new ArgumentNullException(nameof(scopes));

            // Get the account
            IAccount account = await application.GetAccountAsync(accountIdentifier);

            // Special case for guest users as the Guest iod / tenant id are not surfaced.
            if (account == null)
            {
                var accounts = await application.GetAccountsAsync();
                account = accounts.FirstOrDefault(a => a.Username == loginHint);
            }

            AuthenticationResult result;
            if (string.IsNullOrWhiteSpace(tenant))
            {
                result = await application.AcquireTokenSilent(scopes.Except(scopesRequestedByMsalNet), account)
                                           .ExecuteAsync();
            }
            else
            {
                string authority = application.Authority.Replace(new Uri(application.Authority).PathAndQuery, $"/{tenant}/");
                result = await application.AcquireTokenSilent(scopes.Except(scopesRequestedByMsalNet), account)
                                          .WithAuthority(authority)
                                          .ExecuteAsync();
            }
            return result.AccessToken;
        }

        /// <summary>
        /// Adds an account to the token cache from a JWT token and other parameters related to the token cache implementation
        /// </summary>
        private void AddAccountToCacheFromJwt(IEnumerable<string> scopes, JwtSecurityToken jwtToken, ClaimsPrincipal principal, HttpContext httpContext)
        {
            try
            {
                UserAssertion userAssertion;
                IEnumerable<string> requestedScopes;
                if (jwtToken != null)
                {
                    userAssertion = new UserAssertion(jwtToken.RawData, "urn:ietf:params:oauth:grant-type:jwt-bearer");
                    requestedScopes = scopes ?? jwtToken.Audiences.Select(a => $"{a}/.default");
                }
                else
                {
                    throw new ArgumentOutOfRangeException("tokenValidationContext.SecurityToken should be a JWT Token");
                    // TODO: Understand if we could support other kind of client assertions (SAML);
                }

                var application = BuildConfidentialClientApplication(httpContext, principal);

                // .Result to make sure that the cache is filled-in before the controller tries to get access tokens
                var result = application.AcquireTokenOnBehalfOf(requestedScopes.Except(scopesRequestedByMsalNet), userAssertion)
                                        .ExecuteAsync()
                                        .GetAwaiter().GetResult();
            }
            catch (MsalException ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Used in Web APIs (which therefore cannot have an interaction with the user). 
        /// Replies to the client through the HttpReponse by sending a 403 (forbidden) and populating wwwAuthenticateHeaders so that
        /// the client can trigger an iteraction with the user so that the user consents to more scopes
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="scopes">Scopes to consent to</param>
        /// <param name="msalSeviceException"><see cref="MsalUiRequiredException"/> triggering the challenge</param>

        public void ReplyForbiddenWithWwwAuthenticateHeader(HttpContext httpContext, IEnumerable<string> scopes, MsalUiRequiredException msalSeviceException)
        {
            // A user interaction is required, but we are in a Web API, and therefore, we need to report back to the client through an wwww-Authenticate header https://tools.ietf.org/html/rfc6750#section-3.1
            string proposedAction = "consent";
            if (msalSeviceException.ErrorCode == MsalUiRequiredException.InvalidGrantError)
            {
                if (AcceptedTokenVersionIsNotTheSameAsTokenVersion(msalSeviceException))
                {
                    throw msalSeviceException;
                }
            }

            IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "clientId", azureAdOptions.ClientId },
                    { "claims", msalSeviceException.Claims },
                    { "scopes", string.Join(",", scopes) },
                    { "proposedAction", proposedAction }
                };

            string parameterString = string.Join(", ", parameters.Select(p => $"{p.Key}=\"{p.Value}\""));
            string scheme = "Bearer";
            StringValues v = new StringValues($"{scheme} {parameterString}");

            //  StringValues v = new StringValues(new string[] { $"Bearer clientId=\"{jwtToken.Audiences.First()}\", claims=\"{ex.Claims}\", scopes=\" {string.Join(",", scopes)}\"" });
            var httpResponse = httpContext.Response;
            var headers = httpResponse.Headers;
            httpResponse.StatusCode = (int)HttpStatusCode.Forbidden;
            if (headers.ContainsKey(HeaderNames.WWWAuthenticate))
            {
                headers.Remove(HeaderNames.WWWAuthenticate);
            }
            headers.Add(HeaderNames.WWWAuthenticate, v);
        }

        private static bool AcceptedTokenVersionIsNotTheSameAsTokenVersion(MsalUiRequiredException msalSeviceException)
        {
            // Normally app developers should not make decisions based on the internal AAD code
            // however until the STS sends sub-error codes for this error, this is the only
            // way to distinguish the case. 
            // This is subject to change in the future
            return (msalSeviceException.Message.Contains("AADSTS50013"));
        }
    }
}