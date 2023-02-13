using Microsoft.EntityFrameworkCore;
using WebApp_MultiTenant_v2.Models;

namespace WebApp_MultiTenant_v2.DAL
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

        public DbSet<AuthorizedTenant> AuthorizedTenants { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}
