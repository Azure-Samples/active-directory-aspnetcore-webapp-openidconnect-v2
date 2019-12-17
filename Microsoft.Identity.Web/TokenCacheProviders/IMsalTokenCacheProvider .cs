// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
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
        /// <returns></returns>
        Task InitializeAsync(ITokenCache tokenCache);

        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <returns></returns>
        Task ClearAsync();
    }
}
