using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Microsoft.Identity.Web.TokenCacheProviders
{
    public abstract class AbstractMsalTokenCacheProvider : IMsalTokenCacheProvider
    {
        /// <summary>
        /// Azure AD options
        /// </summary>
        protected readonly IOptions<AzureADOptions> _azureAdOptions;

        /// <summary>
        /// Http accessor
        /// </summary>
        protected readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Is the cache an app token cache, or a user token cache?
        /// </summary>
        private bool _isAppTokenCache;

        /// <summary>
        /// Constructor of the abstract token cache provider
        /// </summary>
        /// <param name="azureAdOptions"></param>
        /// <param name="httpContextAccessor"></param>
        protected AbstractMsalTokenCacheProvider(IOptions<AzureADOptions> azureAdOptions, IHttpContextAccessor httpContextAccessor)
        {
            _azureAdOptions = azureAdOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Initializes the token cache serialization.
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize/deserialize</param>
        /// <param name="isAppTokenCache">Is it an app token cache, or a user token cache</param>
        /// <returns></returns>
        public Task InitializeAsync(ITokenCache tokenCache, bool isAppTokenCache)
        {
            tokenCache.SetBeforeAccessAsync(OnBeforeAccessAsync);
            tokenCache.SetAfterAccessAsync(OnAfterAccessAsync);
            tokenCache.SetBeforeWriteAsync(OnBeforeWriteAsync);

            _isAppTokenCache = isAppTokenCache;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Cache key
        /// </summary>
        private string CacheKey
        {
            get
            {
                if (_isAppTokenCache)
                {
                    return $"{_azureAdOptions.Value.ClientId}_AppTokenCache";
                }
                else
                {
                    return _httpContextAccessor.HttpContext.User.GetMsalAccountId();
                }
            }

        }

        private async Task OnAfterAccessAsync(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                if (!string.IsNullOrWhiteSpace(CacheKey))
                {
                    await WriteCacheBytesAsync(CacheKey, args.TokenCache.SerializeMsalV3()).ConfigureAwait(false);
                }
            }
        }

        private async Task OnBeforeAccessAsync(TokenCacheNotificationArgs args)
        {
            if (!string.IsNullOrEmpty(CacheKey))
            {
                byte[] tokenCacheBytes = await ReadCacheBytesAsync(CacheKey).ConfigureAwait(false);
                args.TokenCache.DeserializeMsalV3(tokenCacheBytes, shouldClearExistingCache: true);
            }
        }

        // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        protected virtual Task OnBeforeWriteAsync(TokenCacheNotificationArgs args)
        {
            return Task.CompletedTask;
        }

        public async Task ClearAsync()
        {
            await RemoveKeyAsync(CacheKey).ConfigureAwait(false);
        }

        protected abstract Task WriteCacheBytesAsync(string cacheKey, byte[] bytes);
        protected abstract Task<byte[]> ReadCacheBytesAsync(string cacheKey);

        protected abstract Task RemoveKeyAsync(string cacheKey);
    }
}
