// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;

namespace Microsoft.Identity.Web.TokenCacheProviders
{
    /// <summary>
    /// MSAL token cache provider interface for application token cache
    /// </summary>
    public interface IMsalAppTokenCacheProvider
    {
        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The application token cache instance of MSAL application</param>
        void Initialize(ITokenCache tokenCache);

        /// <summary>
        /// Clears the app token cache for this app
        /// </summary>
        void Clear();
    }
}