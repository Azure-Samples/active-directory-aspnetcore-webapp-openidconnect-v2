using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implentation service
    /// </summary>
    public static class CookieBasedTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddCookieBasedTokenCache(this IServiceCollection services)
        {
            // Token acquisition service
            services.AddSingleton<ITokenCacheProvider, CookieTokenCacheProvider>();
            return services;
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="ITokenCacheProvider"/> for a cookie based token cache implementation
    /// </summary>
    public class CookieTokenCacheProvider : ITokenCacheProvider
    {
        /// <summary>
        /// Get an MSAL.NET Token cache from the HttpContext, and possibly the AuthenticationProperties and Cookies sign-in scheme
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="authenticationProperties">Authentication properties</param>
        /// <param name="signInScheme">Sign-in scheme</param>
        /// <returns>A token cache to use in the application</returns>

        public TokenCache GetCache(HttpContext httpContext, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme)
        {
            if (authenticationProperties!=null)
            {
                return AuthPropertiesTokenCacheHelper.ForCodeRedemption(authenticationProperties);
            }
            else
            {
                return AuthPropertiesTokenCacheHelper.ForApiCalls(httpContext, signInScheme?? AzureADDefaults.CookieScheme);
            }
        }
    }

    public class AuthPropertiesTokenCacheHelper
    {
        private const string TokenCacheKey = ".UserTokenCache";
        private HttpContext _httpContext;
        private ClaimsPrincipal _principal;
        private AuthenticationProperties _authProperties;
        private string _signInScheme;

        private AuthPropertiesTokenCacheHelper(AuthenticationProperties authProperties) : base()
        {
            _authProperties = authProperties;
            TokenCache = new TokenCache();
            TokenCache.SetBeforeAccess(BeforeAccessNotificationWithProperties);
            TokenCache.SetAfterAccess(AfterAccessNotificationWithProperties);
            TokenCache.SetBeforeWrite(BeforeWriteNotification);
        }

        private AuthPropertiesTokenCacheHelper(HttpContext httpContext, string signInScheme) : base()
        {
            _httpContext = httpContext;
            _signInScheme = signInScheme;
            TokenCache = new TokenCache();
            TokenCache.SetBeforeAccess(BeforeAccessNotificationWithContext);
            TokenCache.SetAfterAccess(AfterAccessNotificationWithContext);
            TokenCache.SetBeforeWrite(BeforeWriteNotification);
        }

        public TokenCache TokenCache { get; private set; }

        public static TokenCache ForCodeRedemption(AuthenticationProperties authProperties)
        {
            return new AuthPropertiesTokenCacheHelper(authProperties).TokenCache;
        }

        public static TokenCache ForApiCalls(HttpContext httpContext,
            string signInScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            return new AuthPropertiesTokenCacheHelper(httpContext, signInScheme).TokenCache;
        }

        private void BeforeAccessNotificationWithProperties(TokenCacheNotificationArgs args)
        {
            string cachedTokensText;
            if (_authProperties.Items.TryGetValue(TokenCacheKey, out cachedTokensText))
                TokenCache.Deserialize(Convert.FromBase64String(cachedTokensText));
        }

        private void BeforeAccessNotificationWithContext(TokenCacheNotificationArgs args)
        {
            // Retrieve the auth session with the cached tokens
            var result = _httpContext.AuthenticateAsync(_signInScheme).Result;
            _authProperties = result.Ticket.Properties;
            _principal = result.Ticket.Principal;

            BeforeAccessNotificationWithProperties(args);
        }

        private void AfterAccessNotificationWithProperties(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (args.HasStateChanged)
            {
                var cachedTokens = TokenCache.Serialize();
                _authProperties.Items[TokenCacheKey] = Convert.ToBase64String(cachedTokens);
            }
        }

        private void AfterAccessNotificationWithContext(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (args.HasStateChanged)
            {
                AfterAccessNotificationWithProperties(args);

                var cachedTokens = TokenCache.Serialize();
                _authProperties.Items[TokenCacheKey] = Convert.ToBase64String(cachedTokens);
                _httpContext.SignInAsync(_signInScheme, _principal, _authProperties).Wait();
            }
        }

        private void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }
}
