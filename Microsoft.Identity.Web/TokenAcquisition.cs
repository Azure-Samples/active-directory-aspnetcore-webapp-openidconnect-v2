// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Token acquisition service
    /// </summary>
    public class TokenAcquisition : ITokenAcquisition
    {
        private readonly AzureADOptions _azureAdOptions;
        private readonly ConfidentialClientApplicationOptions _applicationOptions;

        private readonly IMsalAppTokenCacheProvider _appTokenCacheProvider;
        private readonly IMsalUserTokenCacheProvider _userTokenCacheProvider;

        private IConfidentialClientApplication application;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpContext CurrentHttpContext => _httpContextAccessor.HttpContext;

        /// <summary>
        /// Constructor of the TokenAcquisition service. This requires the Azure AD Options to
        /// configure the confidential client application and a token cache provider.
        /// This constructor is called by ASP.NET Core dependency injection
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="appTokenCacheProvider">The App token cache provider</param>
        /// <param name="userTokenCacheProvider">The User token cache provider</param>
        public TokenAcquisition(
            IMsalAppTokenCacheProvider appTokenCacheProvider,
            IMsalUserTokenCacheProvider userTokenCacheProvider,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AzureADOptions> azureAdOptions,
            IOptions<ConfidentialClientApplicationOptions> applicationOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _azureAdOptions = azureAdOptions.Value;
            _applicationOptions = applicationOptions.Value;
            _appTokenCacheProvider = appTokenCacheProvider;
            _userTokenCacheProvider = userTokenCacheProvider;
        }

        /// <summary>
        /// Scopes which are already requested by MSAL.NET. they should not be re-requested;
        /// </summary>
        private readonly string[] _scopesRequestedByMsalNet = new string[]
        {
            OidcConstants.ScopeOpenId,
            OidcConstants.ScopeProfile,
            OidcConstants.ScopeOfflineAccess
        };

        /// <summary>
        /// This handler is executed after authorization code is received (once the user signs-in and consents) during the
        /// <a href='https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow'>Authorization code flow grant flow</a> in a web app.
        /// It uses the code to request an access token from the Microsoft Identity platform and caches the tokens and an entry about the signed-in user's account in the MSAL's token cache.
        /// The access token (and refresh token) provided in the <see cref="AuthorizationCodeReceivedContext"/>, once added to the cache, are then used to acquire more tokens using the
        /// <a href='https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow'>on-behalf-of flow</a> for the signed-in user's account,
        /// in order to call to downstream APIs.
        /// </summary>
        /// <param name="context">The context used when an 'AuthorizationCode' is received over the OpenIdConnect protocol.</param>
        /// <param name="scopes">scopes to request access to</param>
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
        /// And then in the OnAuthorizationCodeRecieved method, call <see cref="AddAccountToCacheFromAuthorizationCodeAsync"/>:
        /// <code>
        /// private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        /// {
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService&lt;ITokenAcquisition&gt;();
        ///    await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, new string[] { "user.read" });
        /// }
        /// </code>
        /// </example>
        public async Task AddAccountToCacheFromAuthorizationCodeAsync(AuthorizationCodeReceivedContext context, IEnumerable<string> scopes)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            try
            {
                // As AcquireTokenByAuthorizationCodeAsync is asynchronous we want to tell ASP.NET core that we are handing the code
                // even if it's not done yet, so that it does not concurrently call the Token endpoint. (otherwise there will be a
                // race condition ending-up in an error from Azure AD telling "code already redeemed")
                context.HandleCodeRedemption();

                // The cache will need the claims from the ID token.
                // If they are not yet in the HttpContext.User's claims, so adding them here.
                if (!context.HttpContext.User.Claims.Any())
                {
                    (context.HttpContext.User.Identity as ClaimsIdentity).AddClaims(context.Principal.Claims);
                }

                var application = GetOrBuildConfidentialClientApplication();

                // Do not share the access token with ASP.NET Core otherwise ASP.NET will cache it and will not send the OAuth 2.0 request in
                // case a further call to AcquireTokenByAuthorizationCodeAsync in the future is required for incremental consent (getting a code requesting more scopes)
                // Share the ID Token though
                var result = await application
                    .AcquireTokenByAuthorizationCode(scopes.Except(_scopesRequestedByMsalNet), context.ProtocolMessage.Code)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                context.HandleCodeRedemption(null, result.IdToken);
            }
            catch (MsalException ex)
            {
                // brentsch - todo, write to a log
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Typically used from a Web App or WebAPI controller, this method retrieves an access token
        /// for a downstream API using the <a href='https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow'>on-behalf-of flow</a>
        /// for the user account that is ascertained from claims are provided in the <see cref="HttpContext.User"/> instance of the <paramref name="context"/> parameter
        /// </summary>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <param name="tenant">Enables overriding of the tenant/account for the same identity. This is useful in the
        /// cases where a given account is guest in other tenants, and you want to acquire tokens for a specific tenant, like where the user is a guest in</param>
        /// <returns>An access token to call the downstream API and populated with this downstream Api's scopes</returns>
        public async Task<string> GetAccessTokenOnBehalfOfUserAsync(
            IEnumerable<string> scopes,
            string tenant = null)
        {
            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            // Use MSAL to get the right token to call the API
            var application = GetOrBuildConfidentialClientApplication();

            // Case of a lazy OBO
            Claim jwtClaim = CurrentHttpContext.User.FindFirst("jwt");
            if (jwtClaim != null)
            {
                (CurrentHttpContext.User.Identity as ClaimsIdentity).RemoveClaim(jwtClaim);
                var result = await application
                    .AcquireTokenOnBehalfOf(scopes.Except(_scopesRequestedByMsalNet), new UserAssertion(jwtClaim.Value))
                    .ExecuteAsync()
                    .ConfigureAwait(false);
                return result.AccessToken;
            }
            else
            {
                return await GetAccessTokenOnBehalfOfUserAsync(application, CurrentHttpContext.User, scopes, tenant).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// In a Web API, adds to the MSAL.NET cache, the account of the user for which a bearer token was received when the Web API was called.
        /// An access token and a refresh token are added to the cache, so that they can then be used to acquire another token on-behalf-of the
        /// same user in order to call to downstream APIs.
        /// </summary>
        /// <param name="tokenValidatedContext">Token validation context passed to the handler of the OnTokenValidated event
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
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService&lt;ITokenAcquisition&gt;();
        ///   tokenAcquisition.AddAccountToCacheFromJwt(context);
        /// };
        /// </code>
        /// </example>
        public Task AddAccountToCacheFromJwtAsync(
            Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext tokenValidatedContext,
            IEnumerable<string> scopes)
        {
            if (tokenValidatedContext == null)
            {
                throw new ArgumentNullException(nameof(tokenValidatedContext));
            }

            return AddAccountToCacheFromJwtAsync(
                scopes,
                tokenValidatedContext.SecurityToken as JwtSecurityToken,
                tokenValidatedContext.Principal);
        }

        /// <summary>
        /// [not recommended] In a Web App, adds, to the MSAL.NET cache, the account of the user authenticating to the Web App.
        /// An On-behalf-of token is added to the cache, so that it can then be used to acquire another token on-behalf-of the
        /// same user in order for the Web App to call a Web APIs.
        /// </summary>
        /// <param name="tokenValidatedContext">Token validation context passed to the handler of the OnTokenValidated event
        /// for the OpenIdConnect middleware</param>
        /// <param name="scopes">[Optional] scopes to pre-request for a downstream API</param>
        /// <remarks>In a Web App, it's preferable to not request an access token, but only a code, and use the <see cref="AddAccountToCacheFromAuthorizationCodeAsync"/></remarks>
        /// <example>
        /// From the configuration of the Authentication of the ASP.NET Core Web API:
        /// <code>OpenIdConnectOptions options;</code>
        ///
        /// Subscribe to the token validated event:
        /// <code>
        ///  options.Events.OnAuthorizationCodeReceived = OnTokenValidated;
        /// </code>
        ///
        /// And then in the OnTokenValidated method, call <see cref="AddAccountToCacheFromJwtAsync(OpenIdConnect.TokenValidatedContext, IEnumerable&lt;string&gt;)"/>:
        /// <code>
        /// private async Task OnTokenValidated(TokenValidatedContext context)
        /// {
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService&lt;ITokenAcquisition&gt;();
        ///  _tokenAcquisition.AddAccountToCacheFromJwt(tokenValidationContext);
        /// }
        /// </code>
        /// </example>
        public Task AddAccountToCacheFromJwtAsync(
            AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext tokenValidatedContext, // JwtBearer.TokenValidatedContext also exists
            IEnumerable<string> scopes = null)
        {
            if (tokenValidatedContext == null)
            {
                throw new ArgumentNullException(nameof(tokenValidatedContext));
            }

            return AddAccountToCacheFromJwtAsync(
                scopes,
                tokenValidatedContext.SecurityToken,
                tokenValidatedContext.Principal);
        }

        /// <summary>
        /// Removes the account associated with context.HttpContext.User from the MSAL.NET cache
        /// </summary>
        /// <param name="context">RedirectContext passed-in to a <see cref="OnRedirectToIdentityProviderForSignOut"/>
        /// Openidconnect event</param>
        /// <returns></returns>
        public async Task RemoveAccountAsync(RedirectContext context)
        {
            ClaimsPrincipal user = context.HttpContext.User;
            IConfidentialClientApplication app = GetOrBuildConfidentialClientApplication();
            IAccount account = await app.GetAccountAsync(context.HttpContext.User.GetMsalAccountId()).ConfigureAwait(false);

            // Workaround for the guest account
            if (account == null)
            {
                var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
                account = accounts.FirstOrDefault(a => a.Username == user.GetLoginHint());
            }

            if (account != null)
            {
                await app.RemoveAsync(account).ConfigureAwait(false);
                _userTokenCacheProvider?.ClearAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates an MSAL Confidential client application if needed
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        private IConfidentialClientApplication GetOrBuildConfidentialClientApplication()
        {
            if (application == null)
            {
                application = BuildConfidentialClientApplication();
            }
            return application;
        }

        /// <summary>
        /// Creates an MSAL Confidential client application
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        private IConfidentialClientApplication BuildConfidentialClientApplication()
        {
            var request = CurrentHttpContext.Request;
            var azureAdOptions = _azureAdOptions;
            var applicationOptions = _applicationOptions;
            string currentUri = UriHelper.BuildAbsolute(
                request.Scheme,
                request.Host,
                request.PathBase,
                azureAdOptions.CallbackPath ?? string.Empty);

            string authority = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/";

            var app = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(applicationOptions)
                .WithRedirectUri(currentUri)
                .WithAuthority(authority)
                .Build();

            // Initialize token cache providers
            _appTokenCacheProvider?.InitializeAsync(app.AppTokenCache);
            _userTokenCacheProvider?.InitializeAsync(app.UserTokenCache);

            return app;
        }

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user described by its claimsPrincipal
        /// </summary>
        /// <param name="application"></param>
        /// <param name="claimsPrincipal">Claims principal for the user on behalf of whom to get a token</param>
        /// <param name="scopes">Scopes for the downstream API to call</param>
        /// <param name="tenant">(optional) Specific tenant for which to acquire a token to access the scopes
        /// on behalf of the user described in the claimsPrincipal</param>
        private async Task<string> GetAccessTokenOnBehalfOfUserAsync(
            IConfidentialClientApplication application,
            ClaimsPrincipal claimsPrincipal,
            IEnumerable<string> scopes,
            string tenant)
        {
            string accountIdentifier = claimsPrincipal.GetMsalAccountId();
            string loginHint = claimsPrincipal.GetLoginHint();
            return await GetAccessTokenOnBehalfOfUserAsync(application, accountIdentifier, scopes, loginHint, tenant).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user which account ID is passed as an argument
        /// </summary>
        /// <param name="application"></param>
        /// <param name="accountIdentifier">User account identifier for which to acquire a token.
        /// See <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></param>
        /// <param name="scopes">Scopes for the downstream API to call</param>
        /// <param name="loginHint"></param>
        /// <param name="tenant"></param>
        private async Task<string> GetAccessTokenOnBehalfOfUserAsync(
            IConfidentialClientApplication application,
            string accountIdentifier,
            IEnumerable<string> scopes,
            string loginHint,
            string tenant)
        {
            if (accountIdentifier == null)
            {
                throw new ArgumentNullException(nameof(accountIdentifier));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            // Get the account
            IAccount account = await application.GetAccountAsync(accountIdentifier).ConfigureAwait(false);

            // Special case for guest users as the Guest oid / tenant id are not surfaced.
            if (account == null)
            {
                if (loginHint == null)
                    throw new ArgumentNullException(nameof(loginHint));
                var accounts = await application.GetAccountsAsync().ConfigureAwait(false);
                account = accounts.FirstOrDefault(a => a.Username == loginHint);
            }

            AuthenticationResult result;
            if (string.IsNullOrWhiteSpace(tenant))
            {
                result = await application
                    .AcquireTokenSilent(scopes.Except(_scopesRequestedByMsalNet), account)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                string authority = application.Authority.Replace(new Uri(application.Authority).PathAndQuery, $"/{tenant}/");
                result = await application
                    .AcquireTokenSilent(scopes.Except(_scopesRequestedByMsalNet), account)
                    .WithAuthority(authority)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }

            return result.AccessToken;
        }

        /// <summary>
        /// Adds an account to the token cache from a JWT token and other parameters related to the token cache implementation
        /// </summary>
        private async Task AddAccountToCacheFromJwtAsync(IEnumerable<string> scopes, JwtSecurityToken jwtToken, ClaimsPrincipal principal)
        {
            try
            {
                UserAssertion userAssertion;
                IEnumerable<string> requestedScopes;
                if (jwtToken != null)
                {
                    // In encrypted tokens, the decrypted token is in the InnerToken
                    string rawData = (jwtToken.InnerToken != null) ? jwtToken.InnerToken.RawData : jwtToken.RawData;
                    userAssertion = new UserAssertion(rawData, "urn:ietf:params:oauth:grant-type:jwt-bearer");
                    requestedScopes = scopes ?? jwtToken.Audiences.Select(a => $"{a}/.default");
                }
                else
                {
                    throw new ArgumentOutOfRangeException("tokenValidationContext.SecurityToken should be a JWT Token");
                    // TODO: Understand if we could support other kind of client assertions (SAML);
                }

                var application = GetOrBuildConfidentialClientApplication();

                // .Result to make sure that the cache is filled-in before the controller tries to get access tokens
                var result = await application
                    .AcquireTokenOnBehalfOf(requestedScopes.Except(_scopesRequestedByMsalNet), userAssertion)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
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
        /// <param name="scopes">Scopes to consent to</param>
        /// <param name="msalServiceException"><see cref="MsalUiRequiredException"/> triggering the challenge</param>
        public void ReplyForbiddenWithWwwAuthenticateHeader(IEnumerable<string> scopes, MsalUiRequiredException msalServiceException)
        {
            // A user interaction is required, but we are in a Web API, and therefore, we need to report back to the client through an wwww-Authenticate header https://tools.ietf.org/html/rfc6750#section-3.1
            string proposedAction = "consent";
            if (msalServiceException.ErrorCode == MsalError.InvalidGrantError)
            {
                if (AcceptedTokenVersionMismatch(msalServiceException))
                {
                    throw msalServiceException;
                }
            }

            string consentUrl = $"{application.Authority}/oauth2/v2.0/authorize?client_id={_azureAdOptions.ClientId}"
                + $"&response_type=code&redirect_uri={application.AppConfig.RedirectUri}"
                + $"&response_mode=query&scope=offline_access%20{string.Join("%20", scopes)}";

            IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "consentUri", consentUrl },
                    { "claims", msalServiceException.Claims },
                    { "scopes", string.Join(",", scopes) },
                    { "proposedAction", proposedAction }
                };

            string parameterString = string.Join(", ", parameters.Select(p => $"{p.Key}=\"{p.Value}\""));
            string scheme = "Bearer";
            StringValues v = new StringValues($"{scheme} {parameterString}");

            var httpResponse = CurrentHttpContext.Response;
            var headers = httpResponse.Headers;
            httpResponse.StatusCode = (int)HttpStatusCode.Forbidden;
            if (headers.ContainsKey(HeaderNames.WWWAuthenticate))
            {
                headers.Remove(HeaderNames.WWWAuthenticate);
            }
            headers.Add(HeaderNames.WWWAuthenticate, v);
        }

        private static bool AcceptedTokenVersionMismatch(MsalUiRequiredException msalSeviceException)
        {
            // Normally app developers should not make decisions based on the internal AAD code
            // however until the STS sends sub-error codes for this error, this is the only
            // way to distinguish the case.
            // This is subject to change in the future
            return (msalSeviceException.Message.Contains("AADSTS50013"));
        }
    }
}