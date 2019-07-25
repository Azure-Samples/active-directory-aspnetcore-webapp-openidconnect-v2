using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Client.TokenCacheProviders;

namespace WebApp_OpenIDConnect_DotNet.Database
{
    public class DataContext : DbContext, ITokenCacheDbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        { }

        public DbSet<AppTokenCache> AppTokenCache { get; set; }
        public DbSet<UserTokenCache> UserTokenCache { get; set; }
    }
}
