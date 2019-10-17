using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    public class TokenAcquisitionB2C : ITokenAcquisition
    {
        private readonly AzureADB2COptions _azureAdB2COptions;
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
        public TokenAcquisitionB2C(
            IMsalAppTokenCacheProvider appTokenCacheProvider,
            IMsalUserTokenCacheProvider userTokenCacheProvider,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AzureADB2COptions> azureAdOptions,
            IOptions<ConfidentialClientApplicationOptions> applicationOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _azureAdB2COptions = azureAdOptions.Value;
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
                context.HandleCodeRedemption();

                if (!context.HttpContext.User.Claims.Any())
                {
                    (context.HttpContext.User.Identity as ClaimsIdentity).AddClaims(context.Principal.Claims);
                }

                var application = GetOrBuildConfidentialClientApplication();

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
                account = accounts.FirstOrDefault();
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
            var azureAdB2COptions = _azureAdB2COptions;
            var applicationOptions = _applicationOptions;
            string currentUri = UriHelper.BuildAbsolute(
                request.Scheme,
                request.Host,
                request.PathBase,
                azureAdB2COptions.CallbackPath ?? string.Empty);

            string authority = $"{azureAdB2COptions.Instance}/tfp/{azureAdB2COptions.Domain}/{azureAdB2COptions.DefaultPolicy}";

            var app = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(applicationOptions)
                .WithRedirectUri(currentUri)
                .WithB2CAuthority(authority)
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
            string currentPolicy = claimsPrincipal.GetPolicyId();

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            IAccount account = null;
            if (accountIdentifier != null)
            {
                account = await application.GetAccountAsync(accountIdentifier).ConfigureAwait(false);
            }

            if (account == null)
            {
                account = GetAccountByPolicy(await application.GetAccountsAsync().ConfigureAwait(false), currentPolicy);
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
                    .WithB2CAuthority(application.Authority)
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

            string consentUrl = $"{application.Authority}/oauth2/v2.0/authorize?client_id={_azureAdB2COptions.ClientId}"
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
        /// Gets an IAccount for the current B2C policy in the user claims
        /// </summary>
        /// <param name="accounts"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        private IAccount GetAccountByPolicy(IEnumerable<IAccount> accounts, string policy)
        {
            foreach (var account in accounts)
            {
                string accountIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
                if (accountIdentifier.EndsWith(policy.ToLower()))
                    return account;
            }

            return null;
        }
    }
}
