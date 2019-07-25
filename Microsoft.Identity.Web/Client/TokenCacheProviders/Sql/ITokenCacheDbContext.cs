using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// The DBContext interface that is used by the TokenCache providers to read and write to a Sql database. Implement this Interface in your database context.
    /// </summary>
    public interface ITokenCacheDbContext
    {
        /// <summary>
        /// The app token cache table
        /// </summary>
        DbSet<AppTokenCache> AppTokenCache { get; set; }

        /// <summary>
        /// The user token cache table
        /// </summary>
        DbSet<UserTokenCache> UserTokenCache { get; set; }

        /// <summary>
        /// from DBContext
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityEntry Entry(object entity);

        int SaveChanges();
    }
}