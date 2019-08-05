// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// The DBContext that is used by the TokenCache providers to read and write to a Sql database.
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
        public DbSet<AppTokenCache> AppTokenCache { get; set; }

        /// <summary>
        /// The user token cache table
        /// </summary>
        public DbSet<UserTokenCache> UserTokenCache { get; set; }
    }
}