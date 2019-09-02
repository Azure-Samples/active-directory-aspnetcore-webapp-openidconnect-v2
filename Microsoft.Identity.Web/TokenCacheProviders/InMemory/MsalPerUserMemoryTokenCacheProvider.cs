// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders.InMemory
{
    /// <summary>
    /// An implementation of token cache for both Confidential and Public clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext.Session to cache data.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalPerUserMemoryTokenCacheProvider : MsalMemoryTokenCacheProvider, IMsalUserTokenCacheProvider
    {
        public MsalPerUserMemoryTokenCacheProvider(IOptions<AzureADOptions> azureAdOptions,
                                    IHttpContextAccessor httpContextAccessor,
                                    IMemoryCache memoryCache,
                                    IOptions<MsalMemoryTokenCacheOptions> cacheOptions) :
              base(azureAdOptions, httpContextAccessor, memoryCache, cacheOptions)
        {

        }

        public async Task InitializeAsync(ITokenCache tokenCache)
        {
            await InitializeAsync(tokenCache, false).ConfigureAwait(false);
        }
    }
}