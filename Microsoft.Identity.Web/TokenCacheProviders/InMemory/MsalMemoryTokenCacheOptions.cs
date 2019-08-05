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
        /// In production, a higher value , upto 90 days is recommended.
        /// </summary>
        /// <value>
        /// The AbsoluteExpiration value.
        /// </value>
        public TimeSpan SlidingExpiration
        {
            get;
            set;
        }

        public MsalMemoryTokenCacheOptions()
        {
            this.SlidingExpiration = TimeSpan.FromHours(12);
        }
    }
}