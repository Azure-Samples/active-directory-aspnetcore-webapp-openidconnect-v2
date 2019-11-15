using Microsoft.EntityFrameworkCore;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.DAL
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

        public DbSet<AuthorizedTenant> AuthorizedTenants { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}
