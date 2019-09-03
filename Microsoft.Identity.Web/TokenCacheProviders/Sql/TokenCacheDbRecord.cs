// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// Represents a token cache entry in database
    /// </summary>
    public class TokenCacheDbRecord
    {
        /// <summary>
        /// Key of the cache in the database
        /// </summary>
        [Key]
        public int TokenCacheId { get; set; }

        /// <summary>
        /// The Appid or ClientId of the app
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// Content of the cache
        /// </summary>
        public byte[] CacheBits { get; set; }

        /// <summary>
        /// Last write date
        /// </summary>
        public DateTime LastWrite { get; set; }

        /// <summary>
        /// Provided here as a precaution against concurrent updates by multiple threads.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}