// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// Represents a user's token cache entry in database
    /// </summary>
    public class UserTokenCache
    {
        /// <summary>
        /// key of the token cache in the database
        /// </summary>
        [Key]
        public int UserTokenCacheId { get; set; }

        /// <summary>
        /// The objectId of the signed-in user's object in Azure AD
        /// </summary>
        public string WebUserUniqueId { get; set; }

        /// <summary>
        /// Cache content
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