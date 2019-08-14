// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Identity.Web.TokenCacheProviders.InMemory
{
    /// <summary>
    /// MSAL's memory token cache options
    /// </summary>
    public class MsalMemoryTokenCacheOptions
    {
        /// <summary>
        /// Gets or sets the value of the duration after which the cache entry will expire unless it's used
        /// This is the duration till the tokens are kept in memory cache.
        /// In production, a higher value , up-to 90 days is recommended.
        /// </summary>
        /// <value>
        /// The SlidingExpiration value.
        /// </value>
        public TimeSpan SlidingExpiration
        {
            get;
            set;
        }

        /// <summary>Initializes a new instance of the <see cref="MsalMemoryTokenCacheOptions"/> class.
        /// By default, the sliding expiration is set for 14 days.</summary>
        public MsalMemoryTokenCacheOptions()
        {
            this.SlidingExpiration = TimeSpan.FromDays(14);
        }
    }
}