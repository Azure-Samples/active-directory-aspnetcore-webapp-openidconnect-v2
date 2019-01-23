using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implentation service
    /// </summary>
    public static class InMemoryTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddInMemoryTokenCache(this IServiceCollection services)
        {
            // Token acquisition service
            services.AddSingleton<ITokenCacheProvider, InMemoryTokenCacheProvider>();
            return services;
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="ITokenCacheProvider"/> for a cookie based token cache implementation
    /// </summary>
    class InMemoryTokenCacheProvider : ITokenCacheProvider
    {
        InMemoryTokenCacheHelper helper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cache"></param>
        public InMemoryTokenCacheProvider(IMemoryCache cache)
        {
            memoryCache = cache;
        }

        IMemoryCache memoryCache;

        /// <summary>
        /// Get an MSAL.NET Token cache from the HttpContext, and possibly the AuthenticationProperties and Cookies sign-in scheme
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="authenticationProperties">Authentication properties</param>
        /// <param name="signInScheme">Sign-in scheme</param>
        /// <returns>A token cache to use in the application</returns>

        public ITokenCache GetCache(HttpContext httpContext, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme)
        {
            string userId = claimsPrincipal.GetMsalAccountId();
            helper = new InMemoryTokenCacheHelper(null, userId, httpContext, memoryCache);
            return helper.GetMsalCacheInstance();
        }

        public void EnableSerialization(ITokenCache userTokenCache, HttpContext httpContext, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme = null)
        {
            string userId = claimsPrincipal.GetMsalAccountId();
            helper = new InMemoryTokenCacheHelper(userTokenCache, userId, httpContext, memoryCache);
            helper.GetMsalCacheInstance();
        }
    }

    public class InMemoryTokenCacheHelper
    {
        string UserId = string.Empty;
        string CacheId = string.Empty;
        IMemoryCache memoryCache;


        ITokenCache cache = new TokenCache();

        public InMemoryTokenCacheHelper(ITokenCache userTokenCache, string userId, HttpContext httpcontext, IMemoryCache aspnetInMemoryCache)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            memoryCache = aspnetInMemoryCache;
            if (userTokenCache != null)
            {
                this.cache = userTokenCache;
            }
            Load();
        }

        public ITokenCache GetMsalCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return cache;
        }

        public void Load()
        {
            byte[] blob;
            if (memoryCache.TryGetValue(CacheId, out blob))
            {
                cache.Deserialize(blob);
            }

        }

        public void Persist()
        {
            // Reflect changes in the persistent store
            byte[] blob = cache.Serialize();
            memoryCache.Set(CacheId, blob);
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
                Persist();
        }
    }
}
