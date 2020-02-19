// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        private readonly MicrosoftIdentityOptions _microsoftIdentityOptions;
        private readonly ConfidentialClientApplicationOptions _applicationOptions;

        private readonly IMsalTokenCacheProvider _tokenCacheProvider;

        private IConfidentialClientApplication application;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpContext CurrentHttpContext => _httpContextAccessor.HttpContext;

        /// <summary>
        /// Constructor of the TokenAcquisition service. This requires the Azure AD Options to
        /// configure the confidential client application and a token cache provider.
        /// This constructor is called by ASP.NET Core dependency injection
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="tokenCacheProvider">The App token cache provider</param>
        /// <param name="userTokenCacheProvider">The User token cache provider</param>
        public TokenAcquisition(
            IMsalTokenCacheProvider tokenCacheProvider,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MicrosoftIdentityOptions> microsoftIdentityOptions,
            IOptions<ConfidentialClientApplicationOptions> applicationOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _microsoftIdentityOptions = microsoftIdentityOptions.Value;
            _applicationOptions = applicationOptions.Value;
            _tokenCacheProvider = tokenCacheProvider;
        }

        /// <summary>
        /// Scopes which are already requested by MSAL.NET. They should not be re-requested;
        /// </summary>
        private readonly string[] _scopesRequestedByMsal = new string[]
        {
            OidcConstants.ScopeOpenId,
            OidcConstants.ScopeProfile,
            OidcConstants.ScopeOfflineAccess
        };

        /// <summary>
        /// This handler is executed after the authorization code is received (once the user signs-in and consents) during the
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
        public async Task AddAccountToCacheFromAuthorizationCodeAsync(
            AuthorizationCodeReceivedContext context,
            IEnumerable<string> scopes)
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
                // If they are not yet in the HttpContext.User's claims, add them here.
                if (!context.HttpContext.User.Claims.Any())
                {
                    (context.HttpContext.User.Identity as ClaimsIdentity).AddClaims(context.Principal.Claims);
                }

                var application = GetOrBuildConfidentialClientApplication();

                // Do not share the access token with ASP.NET Core otherwise ASP.NET will cache it and will not send the OAuth 2.0 request in
                // case a further call to AcquireTokenByAuthorizationCodeAsync in the future is required for incremental consent (getting a code requesting more scopes)
                // Share the ID Token though
                var result = await application
                    .AcquireTokenByAuthorizationCode(scopes.Except(_scopesRequestedByMsal), context.ProtocolMessage.Code)
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
        /// for a downstream API using;
        /// 1) the token cache (for Web Apps and Web APIs) if a token exists in the cache
        /// 2) or the <a href='https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow'>on-behalf-of flow</a>
        /// in Web APIs, for the user account that is ascertained from claims are provided in the <see cref="HttpContext.User"/> 
        /// instance of the current HttpContext
        /// </summary>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <param name="tenant">Enables overriding of the tenant/account for the same identity. This is useful in the
        /// cases where a given account is a guest in other tenants, and you want to acquire tokens for a specific tenant, like where the user is a guest in</param>
        /// <returns>An access token to call the downstream API and populated with this downstream Api's scopes</returns>
        /// <remarks>Calling this method from a Web API supposes that you have previously called, 
        /// in a method called by JwtBearerOptions.Events.OnTokenValidated, the HttpContextExtensions.StoreTokenUsedToCallWebAPI method
        /// passing the validated token (as a JwtSecurityToken). Calling it from a Web App supposes that
        /// you have previously called AddAccountToCacheFromAuthorizationCodeAsync from a method called by
        /// OpenIdConnectOptions.Events.OnAuthorizationCodeReceived</remarks>
        [Obsolete("This method has been deprecated, please use the GetAccessTokenForUserAsync() method instead.")]
        public async Task<string> GetAccessTokenOnBehalfOfUserAsync(
            IEnumerable<string> scopes,
            string tenant = null)
        {
            return await GetAccessTokenForUserAsync(scopes, tenant);
        }

        /// <summary>
        /// Typically used from a Web App or WebAPI controller, this method retrieves an access token
        /// for a downstream API using;
        /// 1) the token cache (for Web Apps and Web APis) if a token exists in the cache
        /// 2) or the <a href='https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow'>on-behalf-of flow</a>
        /// in Web APIs, for the user account that is ascertained from claims are provided in the <see cref="HttpContext.User"/> 
        /// instance of the current HttpContext
        /// </summary>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <param name="tenant">Enables overriding of the tenant/account for the same identity. This is useful in the
        /// cases where a given account is guest in other tenants, and you want to acquire tokens for a specific tenant, like where the user is a guest in</param>
        /// <returns>An access token to call the downstream API and populated with this downstream Api's scopes</returns>
        /// <remarks>Calling this method from a Web API supposes that you have previously called, 
        /// in a method called by JwtBearerOptions.Events.OnTokenValidated, the HttpContextExtensions.StoreTokenUsedToCallWebAPI method
        /// passing the validated token (as a JwtSecurityToken). Calling it from a Web App supposes that
        /// you have previously called AddAccountToCacheFromAuthorizationCodeAsync from a method called by
        /// OpenIdConnectOptions.Events.OnAuthorizationCodeReceived</remarks>
        public async Task<string> GetAccessTokenForUserAsync(
            IEnumerable<string> scopes,
            string tenant = null)
        {
            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            // Use MSAL to get the right token to call the API
            var application = GetOrBuildConfidentialClientApplication();
            string accessToken;

            try
            {
                accessToken = await GetAccessTokenOnBehalfOfUserFromCacheAsync(application, CurrentHttpContext.User, scopes, tenant)
                    .ConfigureAwait(false);
            }

            catch (MsalUiRequiredException ex)
            {
                // GetAccessTokenForUserAsync is an abstraction that can be called from a Web App or a Web API
                Debug.WriteLine(ex.Message);

                // to get a token for a Web API on behalf of the user, but not necessarily with the on behalf of OAuth2.0
                // flow as this one only applies to Web APIs.
                JwtSecurityToken validatedToken = CurrentHttpContext.GetTokenUsedToCallWebAPI();

                // Case of Web APIs: we need to do an on-behalf-of flow
                if (validatedToken != null)
                {
                    // In the case the token is a JWE (encrypted token), we use the decrypted token.
                    string tokenUsedToCallTheWebApi = validatedToken.InnerToken == null ? validatedToken.RawData
                                                : validatedToken.InnerToken.RawData;
                    var result = await application
                                        .AcquireTokenOnBehalfOf(scopes.Except(_scopesRequestedByMsal),
                                                                new UserAssertion(tokenUsedToCallTheWebApi))
                                        .ExecuteAsync()
                                        .ConfigureAwait(false);
                    accessToken = result.AccessToken;
                }

                // Case of the Web App: we let the MsalUiRequiredException be caught by the 
                // AuthorizeForScopesAttribute exception filter so that the user can consent, do 2FA, etc ...
                else
                {
                    throw;
                }
            }

            return accessToken;
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
            IAccount account = null;

            // For B2C, we should remove all accounts of the user regardless the user flow
            if (_microsoftIdentityOptions.IsB2C)
            {
                var b2cAccounts = await app.GetAccountsAsync().ConfigureAwait(false);

                foreach (var b2cAccount in b2cAccounts)
                {
                    await app.RemoveAsync(b2cAccount).ConfigureAwait(false);
                }

                _tokenCacheProvider?.ClearAsync().ConfigureAwait(false);
            }

            else
            {
                account = await app.GetAccountAsync(context.HttpContext.User.GetMsalAccountId()).ConfigureAwait(false);

                // Workaround for the guest account
                if (account == null)
                {
                    var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
                    account = accounts.FirstOrDefault(a => a.Username == user.GetLoginHint());
                }

                if (account != null)
                {
                    await app.RemoveAsync(account).ConfigureAwait(false);
                    _tokenCacheProvider?.ClearAsync().ConfigureAwait(false);
                }
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
            var microsoftIdentityOptions = _microsoftIdentityOptions;
            var applicationOptions = _applicationOptions;
            string currentUri = UriHelper.BuildAbsolute(
                request.Scheme,
                request.Host,
                request.PathBase,
                microsoftIdentityOptions.CallbackPath.Value ?? string.Empty);

            if (!applicationOptions.Instance.EndsWith("/"))
                applicationOptions.Instance += "/";

            string authority ;
            IConfidentialClientApplication app = null;

            if (microsoftIdentityOptions.IsB2C)
            {
                authority = $"{applicationOptions.Instance}tfp/{microsoftIdentityOptions.Domain}/{microsoftIdentityOptions.DefaultUserFlow}";
                app = ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(applicationOptions)
                    .WithRedirectUri(currentUri)
                    .WithB2CAuthority(authority)
                    .Build();
            }
            else
            {
                authority = $"{applicationOptions.Instance}{applicationOptions.TenantId}/";
                app = ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(applicationOptions)
                    .WithRedirectUri(currentUri)
                    .WithAuthority(authority)
                    .Build();
            }

            // Initialize token cache providers
            _tokenCacheProvider?.InitializeAsync(app.AppTokenCache);
            _tokenCacheProvider?.InitializeAsync(app.UserTokenCache);

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
        private async Task<string> GetAccessTokenOnBehalfOfUserFromCacheAsync(
            IConfidentialClientApplication application,
            ClaimsPrincipal claimsPrincipal,
            IEnumerable<string> scopes,
            string tenant)
        {
            // Gets MsalAccountId for AAD and B2C scenarios
            string accountIdentifier = claimsPrincipal.GetMsalAccountId();
            string loginHint = claimsPrincipal.GetLoginHint();
            IAccount account = null;

            if (accountIdentifier != null)
            {
                account = await application.GetAccountAsync(accountIdentifier).ConfigureAwait(false);

                // Special case for guest users as the Guest oid / tenant id are not surfaced.
                // B2C should not follow this logic since loginHint is not present
                if (!_microsoftIdentityOptions.IsB2C && account == null)
                {
                    if (loginHint == null)
                        throw new ArgumentNullException(nameof(loginHint));

                    var accounts = await application.GetAccountsAsync().ConfigureAwait(false);
                    account = accounts.FirstOrDefault(a => a.Username == loginHint);
                }
            }

            // If it is B2C and could not get an account (most likely because there is no tid claims), try to get it by user flow
            if (_microsoftIdentityOptions.IsB2C && account == null)
            {
                string currentUserFlow = claimsPrincipal.GetUserFlowId();
                account = GetAccountByUserFlow(await application.GetAccountsAsync().ConfigureAwait(false), currentUserFlow);
            }

            return await GetAccessTokenOnBehalfOfUserFromCacheAsync(application, account, scopes, tenant).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an access token for a downstream API on behalf of the user which account is passed as an argument
        /// </summary>
        /// <param name="application"></param>
        /// <param name="account">User IAccount for which to acquire a token.
        /// See <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></param>
        /// <param name="scopes">Scopes for the downstream API to call</param>
        /// <param name="tenant"></param>
        private async Task<string> GetAccessTokenOnBehalfOfUserFromCacheAsync(
            IConfidentialClientApplication application,
            IAccount account,
            IEnumerable<string> scopes,
            string tenant)
        {
            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            AuthenticationResult result;
            // Acquire an access token as a B2C authority
            if (_microsoftIdentityOptions.IsB2C)
            {
                string authority = application.Authority.Replace(
                    new Uri(application.Authority).PathAndQuery,
                    $"/tfp/{_microsoftIdentityOptions.Domain}/{_microsoftIdentityOptions.DefaultUserFlow}");

                result = await application
                    .AcquireTokenSilent(scopes.Except(_scopesRequestedByMsal), account)
                    .WithB2CAuthority(authority)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                return result.AccessToken;
            }

            else if (!string.IsNullOrWhiteSpace(tenant))
            {
                // Acquire an access token as another AAD authority
                string authority = application.Authority.Replace(new Uri(application.Authority).PathAndQuery, $"/{tenant}/");
                result = await application
                    .AcquireTokenSilent(scopes.Except(_scopesRequestedByMsal), account)
                    .WithAuthority(authority)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
                return result.AccessToken;
            }
            else
            {
                result = await application
                   .AcquireTokenSilent(scopes.Except(_scopesRequestedByMsal), account)
                   .ExecuteAsync()
                   .ConfigureAwait(false);
                return result.AccessToken;
            }
        }

        /// <summary>
        /// Used in Web APIs (which therefore cannot have an interaction with the user).
        /// Replies to the client through the HttpResponse by sending a 403 (forbidden) and populating wwwAuthenticateHeaders so that
        /// the client can trigger an interaction with the user so that the user consents to more scopes
        /// </summary>
        /// <param name="scopes">Scopes to consent to</param>
        /// <param name="msalServiceException"><see cref="MsalUiRequiredException"/> triggering the challenge</param>
        public void ReplyForbiddenWithWwwAuthenticateHeader(IEnumerable<string> scopes, MsalUiRequiredException msalServiceException)
        {
            // A user interaction is required, but we are in a Web API, and therefore, we need to report back to the client through a www-Authenticate header https://tools.ietf.org/html/rfc6750#section-3.1
            string proposedAction = "consent";
            if (msalServiceException.ErrorCode == MsalError.InvalidGrantError)
            {
                if (AcceptedTokenVersionMismatch(msalServiceException))
                {
                    throw msalServiceException;
                }
            }

            string consentUrl = $"{application.Authority}/oauth2/v2.0/authorize?client_id={_applicationOptions.ClientId}"
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

        /// <summary>
        /// Gets an IAccount for the current B2C user flow in the user claims
        /// </summary>
        /// <param name="accounts"></param>
        /// <param name="userFlow"></param>
        /// <returns></returns>
        private IAccount GetAccountByUserFlow(IEnumerable<IAccount> accounts, string userFlow)
        {
            foreach (var account in accounts)
            {
                string accountIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
                if (accountIdentifier.EndsWith(userFlow.ToLower()))
                    return account;
            }

            return null;
        }
    }
}