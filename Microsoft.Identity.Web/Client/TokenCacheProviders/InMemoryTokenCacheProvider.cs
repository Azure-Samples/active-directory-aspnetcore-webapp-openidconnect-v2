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
        /// <param name="claimsPrincipal">Account for which to get the cache</param>
        /// <returns>A token cache to use in the application</returns>
        public void EnableSerialization(ITokenCache userTokenCache, HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            string userId = claimsPrincipal.GetMsalAccountId();
            helper = new InMemoryTokenCacheHelper(userTokenCache, userId, httpContext, memoryCache);
            helper.GetMsalCacheInstance();
        }
    }

    public class InMemoryTokenCacheHelper
    {
        private readonly string UserId;
        private readonly string CacheId;
        private readonly IMemoryCache memoryCache;

        private readonly ITokenCache cache;

        public InMemoryTokenCacheHelper(ITokenCache tokenCache, string userId, HttpContext httpcontext, IMemoryCache aspnetInMemoryCache)
        {
            // not object, we want the SUB
            cache = tokenCache;
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            memoryCache = aspnetInMemoryCache;
        }

        public ITokenCache GetMsalCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            return cache;
        }

        public void Load()
        {
            byte[] blob;
            if (memoryCache.TryGetValue(CacheId, out blob))
            {
                cache.DeserializeMsalV3(blob);
            }

        }

        public void Persist()
        {
            // Reflect changes in the persistent store
            byte[] blob = cache.SerializeMsalV3();
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
