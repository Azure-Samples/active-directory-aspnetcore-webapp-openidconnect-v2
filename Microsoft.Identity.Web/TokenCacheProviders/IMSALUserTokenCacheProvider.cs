// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;

namespace Microsoft.Identity.Web.TokenCacheProviders
{
    /// <summary>
    /// MSAL token cache provider interface for user accounts
    /// </summary>
    public interface IMsalUserTokenCacheProvider
    {
        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        void Initialize(ITokenCache tokenCache);

        /// <summary>
        /// Clears the token cache for this user
        /// </summary>
        /// <param name="accountId">ID of the account to remove from the cache</param>
        void Clear(string accountId);
    }
}