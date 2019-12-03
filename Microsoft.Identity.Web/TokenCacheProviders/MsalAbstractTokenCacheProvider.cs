// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders
{
    /// <summary></summary>
    /// <seealso cref="Microsoft.Identity.Web.TokenCacheProviders.IMsalTokenCacheProvider" />
    public abstract class MsalAbstractTokenCacheProvider : IMsalTokenCacheProvider
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
        /// Constructor of the abstract token cache provider
        /// </summary>
        /// <param name="azureAdOptions"></param>
        /// <param name="httpContextAccessor"></param>
        protected MsalAbstractTokenCacheProvider(IOptions<AzureADOptions> azureAdOptions, IHttpContextAccessor httpContextAccessor)
        {
            _azureAdOptions = azureAdOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Initializes the token cache serialization.
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize/deserialize</param>
        /// <returns></returns>
        public Task InitializeAsync(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccessAsync(OnBeforeAccessAsync);
            tokenCache.SetAfterAccessAsync(OnAfterAccessAsync);
            tokenCache.SetBeforeWriteAsync(OnBeforeWriteAsync);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Cache key
        /// </summary>
        private string GetCacheKey(bool isAppTokenCache)
        {
            if (isAppTokenCache)
            {
                return $"{_azureAdOptions.Value.ClientId}_AppTokenCache";
            }
            else
            {
                    // In the case of Web Apps, the cache key is the user account Id, and the expectation is that AcquireTokenSilent
                    // should return a token otherwise this might require a challenge
                    // In the case Web APIs, the token cache key is a hash of the access token used to call the Web API
                    JwtSecurityToken jwtSecurityToken = _httpContextAccessor.HttpContext.GetTokenUsedToCallWebAPI();
                    return (jwtSecurityToken != null) ? jwtSecurityToken.RawSignature
                                                                      : _httpContextAccessor.HttpContext.User.GetMsalAccountId();
            }
        }

        /// <summary>
        /// Raised AFTER MSAL added the new token in its in-memory copy of the cache.
        /// This notification is called every time MSAL accessed the cache, not just when a write took place:
        /// If MSAL's current operation resulted in a cache change, the property TokenCacheNotificationArgs.HasStateChanged will be set to true.
        /// If that is the case, we call the TokenCache.SerializeMsalV3() to get a binary blob representing the latest cache content – and persist it.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private async Task OnAfterAccessAsync(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                string cacheKey = GetCacheKey(args.IsApplicationCache);
                if (!string.IsNullOrWhiteSpace(cacheKey))
                {
                    await WriteCacheBytesAsync(cacheKey, args.TokenCache.SerializeMsalV3()).ConfigureAwait(false);
                }
            }
        }

        private async Task OnBeforeAccessAsync(TokenCacheNotificationArgs args)
        {
            string cacheKey = GetCacheKey(args.IsApplicationCache);

            if (!string.IsNullOrEmpty(cacheKey))
            {
                byte[] tokenCacheBytes = await ReadCacheBytesAsync(cacheKey).ConfigureAwait(false);
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
            // This is here a user token cache
            await RemoveKeyAsync(GetCacheKey(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Method to be implemented by concrete cache serializers to write the cache bytes
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="bytes">Bytes to write</param>
        /// <returns></returns>
        protected abstract Task WriteCacheBytesAsync(string cacheKey, byte[] bytes);

        /// <summary>
        /// Method to be implemented by concrete cache serializers to Read the cache bytes
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <returns>Read bytes</returns>
        protected abstract Task<byte[]> ReadCacheBytesAsync(string cacheKey);

        /// <summary>
        /// Method to be implemented by concrete cache serializers to remove an entry from the cache
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        protected abstract Task RemoveKeyAsync(string cacheKey);
    }
}
