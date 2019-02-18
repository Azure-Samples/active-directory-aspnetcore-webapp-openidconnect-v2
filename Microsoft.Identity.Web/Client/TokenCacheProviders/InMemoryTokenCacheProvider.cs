using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// Provides an implementation of <see cref="ITokenCacheProvider"/> for a cookie based token cache implementation
    /// </summary>
    public class InMemoryTokenCacheProvider : ITokenCacheProvider
    {
        private InMemoryTokenCacheHelper helper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cache"></param>
        public InMemoryTokenCacheProvider(IMemoryCache cache)
        {
            memoryCache = cache;
        }

        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Get an MSAL.NET Token cache from the HttpContext, and possibly the AuthenticationProperties and Cookies sign-in scheme
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="authenticationProperties">Authentication properties</param>
        /// <param name="signInScheme">Sign-in scheme</param>
        /// <returns>A token cache to use in the application</returns>

        public TokenCache GetCache(HttpContext httpContext, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme)
        {
            string userId = claimsPrincipal.GetMsalAccountId();
            helper = new InMemoryTokenCacheHelper(userId, httpContext, memoryCache);
            return helper.GetMsalCacheInstance();
        }
    }

    public class InMemoryTokenCacheHelper
    { 
        private readonly string UserId;
        private readonly string CacheId;
        private readonly IMemoryCache memoryCache;

        private readonly TokenCache cache = new TokenCache();

        public InMemoryTokenCacheHelper(string userId, HttpContext httpcontext, IMemoryCache aspnetInMemoryCache)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            memoryCache = aspnetInMemoryCache;
            Load();
        }

        public TokenCache GetMsalCacheInstance()
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
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
                Persist();
        }
    }
}
