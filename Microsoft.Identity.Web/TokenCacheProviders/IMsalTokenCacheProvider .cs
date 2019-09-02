﻿using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders
{
    /// <summary>
    /// MSAL token cache provider interface.
    /// </summary>
    public interface IMsalTokenCacheProvider
    {
        /// <summary>
        /// Initializes a token cache (which can be a user token cache or an app token cache)
        /// </summary>
        /// <param name="tokenCache">Token cache for which to initialize the serialization</param>
        /// <param name="isAppTokenCache">Is the token cache an App token cache or
        /// a user token cache</param>
        /// <returns></returns>
        Task InitializeAsync(ITokenCache tokenCache, bool isAppTokenCache);

        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <returns></returns>
        Task ClearAsync();
    }
}
