// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// The DBContext that is used by the TokenCache providers to read and write to a Sql server database.
    /// </summary>
    public class TokenCacheDbContext : DbContext
    {
        /// <summary>
        /// Entity Framework DbContext for the token cache database
        /// </summary>
        /// <param name="options"></param>
        public TokenCacheDbContext(DbContextOptions<TokenCacheDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// The app token cache table
        /// </summary>
        public DbSet<TokenCacheDbRecord> Records { get; set; }
    }
}