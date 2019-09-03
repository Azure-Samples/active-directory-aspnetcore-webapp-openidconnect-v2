// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Web.TokenCacheProviders.InMemory
{
    /// <summary>
    /// An implementation of token cache for both Confidential and Public clients backed by MemoryCache.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalMemoryTokenCacheProvider : MsalAbstractTokenCacheProvider
    {
        /// <summary>
        /// .NET Core Memory cache
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Msal memory token cache options
        /// </summary>
        private readonly MsalMemoryTokenCacheOptions _cacheOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="azureAdOptions"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="memoryCache"></param>
        /// <param name="cacheOptions"></param>
        public MsalMemoryTokenCacheProvider(IOptions<AzureADOptions> azureAdOptions,
                                            IHttpContextAccessor httpContextAccessor,
                                            IMemoryCache memoryCache,
                                            IOptions<MsalMemoryTokenCacheOptions> cacheOptions) :
            base(azureAdOptions, httpContextAccessor)
        {
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value;
        }

        protected override Task RemoveKeyAsync(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
            return Task.CompletedTask;
        }

        protected override Task<byte[]> ReadCacheBytesAsync(string cacheKey)
        {
            byte[] tokenCacheBytes = (byte[])_memoryCache.Get(cacheKey);
            return Task.FromResult(tokenCacheBytes);
        }

        protected override Task WriteCacheBytesAsync(string cacheKey, byte[] bytes)
        {
            _memoryCache.Set(cacheKey, bytes, _cacheOptions.SlidingExpiration);
            return Task.CompletedTask;
        }
    }
}
