// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace Microsoft.Identity.Web.TokenCacheProviders.InMemory
{
    /// <summary>
    /// An implementation of token cache for both Confidential and Public clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext.Session to cache data.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalPerUserMemoryTokenCacheProvider : IMsalUserTokenCacheProvider
    {
        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        internal IMemoryCache _memoryCache;

        private readonly MsalMemoryTokenCacheOptions _cacheOptions;

        /// <summary>
        /// Enables the singleton object to access the right HttpContext
        /// </summary>
        private IHttpContextAccessor httpContextAccessor;

        /// <summary>Initializes a new instance of the <see cref="MsalPerUserMemoryTokenCacheProvider"/> class.</summary>
        /// <param name="cache">The IMemoryCache cache instance.</param>
        /// <param name="option">The MsalMemoryTokenCacheOptions options provided via config.</param>
        /// <param name="azureAdOptionsAccessor">The azure ad options accessor.</param>
        public MsalPerUserMemoryTokenCacheProvider(IMemoryCache memoryCache,
            MsalMemoryTokenCacheOptions option,
            IHttpContextAccessor httpContextAccessor)
        {
            // TODO: Check if httpContextAccessor && httpContextAccessor.HttpContext is not null first ?
            this.httpContextAccessor = httpContextAccessor;
            this._memoryCache = memoryCache;

            if (option == null)
            {
                _cacheOptions = new MsalMemoryTokenCacheOptions();
            }
            else
            {
                _cacheOptions = option;
            }
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of the MSAL application instance</param>
        public void Initialize(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(this.UserTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(this.UserTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(this.UserTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear(string accountId)
        {
            _memoryCache.Remove(accountId);
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                string cacheKey = httpContextAccessor.HttpContext.User.GetMsalAccountId();

                if (string.IsNullOrWhiteSpace(cacheKey))
                    return;

                // Ideally, methods that load and persist should be thread safe. MemoryCache.Get() is thread safe.
                _memoryCache.Set(cacheKey, args.TokenCache.SerializeMsalV3(), _cacheOptions.SlidingExpiration);
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it
        /// changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            string cacheKey = httpContextAccessor.HttpContext.User.GetMsalAccountId();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            byte[] tokenCacheBytes = (byte[])_memoryCache.Get(cacheKey);
            args.TokenCache.DeserializeMsalV3(tokenCacheBytes, shouldClearExistingCache: true);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a MemoryCache, whose methods are threads safe, we need not to do anything in this handler.
        }
    }
}