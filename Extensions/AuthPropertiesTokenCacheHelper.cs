using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

namespace TodoListService.Extensions
{
    public class AuthPropertiesTokenCacheHelper
    {
        private const string TokenCacheKey = ".UserTokenCache";

        private HttpContext _httpContext;
        private ClaimsPrincipal _principal;
        private AuthenticationProperties _authProperties;
        private string _signInScheme;
        public TokenCache TokenCache { get; private set; }

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
            {
                var cachedTokens = Convert.FromBase64String(cachedTokensText);
                TokenCache.Deserialize(cachedTokens);
            }
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
            if (TokenCache.HasStateChanged)
            {
                var cachedTokens = TokenCache.Serialize();
                var cachedTokensText = Convert.ToBase64String(cachedTokens);
                _authProperties.Items[TokenCacheKey] = cachedTokensText;
            }
        }

        private void AfterAccessNotificationWithContext(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (TokenCache.HasStateChanged)
            {
                AfterAccessNotificationWithProperties(args);

                var cachedTokens = TokenCache.Serialize();
                var cachedTokensText = Convert.ToBase64String(cachedTokens);
                _authProperties.Items[TokenCacheKey] = cachedTokensText;
                _httpContext.SignInAsync(_signInScheme, _principal, _authProperties).Wait();
            }
        }

        private void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }
}
