// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;

namespace Microsoft.Identity.Web.TokenCacheProviders.InMemory
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext to cache data.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalAppMemoryTokenCacheProvider : IMsalAppTokenCacheProvider
    {
        /// <summary>
        /// The application cache key
        /// </summary>
        internal string _appCacheId;

        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        internal IMemoryCache _memoryCache;

        private readonly MsalMemoryTokenCacheOptions _cacheOptions;

        /// <summary>
        /// The client id of the app whose cache we are maintaining.
        /// </summary>
        private readonly string _appId;

        /// <summary>Initializes a new instance of the <see cref="MsalAppMemoryTokenCacheProvider"/> class.</summary>
        /// <param name="cache">The IMemoryCache cache instance.</param>
        /// <param name="option">The MsalMemoryTokenCacheOptions options provided via config.</param>
        /// <param name="azureAdOptionsAccessor">The azure ad options accessor.</param>
        /// <exception cref="ArgumentNullException">AzureADOptions - The app token cache needs the '{nameof(AzureADOptions)}' section in configuration, populated with clientId to initialize..</exception>
        public MsalAppMemoryTokenCacheProvider(IMemoryCache cache,
            MsalMemoryTokenCacheOptions option,
            IOptionsMonitor<AzureADOptions> azureAdOptionsAccessor)
        {
            if (option == null)
            {
                _cacheOptions = new MsalMemoryTokenCacheOptions();
            }
            else
            {
                _cacheOptions = option;
            }

            if (azureAdOptionsAccessor.CurrentValue == null && string.IsNullOrWhiteSpace(azureAdOptionsAccessor.CurrentValue.ClientId))
            {
                throw new ArgumentNullException(nameof(AzureADOptions), $"The app token cache needs the '{nameof(AzureADOptions)}' section in configuration, populated with clientId to initialize.");
            }

            _appId = azureAdOptionsAccessor.CurrentValue.ClientId;
            _memoryCache = cache;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        public void Initialize(ITokenCache tokenCache)
        {
            _appCacheId = _appId + "_AppTokenCache";

            tokenCache.SetBeforeAccess(AppTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(AppTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(AppTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// If you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a MemoryCache, whose methods are threads safe, we need not to do anything in this handler.
        }

        /// <summary>
        /// Clears the token cache for this app.
        /// </summary>
        public void Clear()
        {
            _memoryCache.Remove(_appCacheId);
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            // Load the token cache from memory
            byte[] tokenCacheBytes = (byte[])_memoryCache.Get(_appCacheId);

            // In web apps and web APIs, it's crucial to clear the existing content of the cache
            args.TokenCache.DeserializeMsalV3(tokenCacheBytes, shouldClearExistingCache: true);
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                // Reflect changes in the persistence store
                _memoryCache.Set(_appCacheId, args.TokenCache.SerializeMsalV3(), _cacheOptions.SlidingExpiration);
            }
        }
    }
}