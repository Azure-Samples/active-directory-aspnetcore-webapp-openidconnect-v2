using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using WebApp_OpenIDConnect_DotNet.DAL;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SampleDbContext dbContext;

        public HomeController(SampleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var authorizedTenants = dbContext.AuthorizedTenants.Where(x => x.TenantId != null && x.AuthorizedOn != null).ToList();
            return View(authorizedTenants);
        }

        public IActionResult DeleteTenant(string id)
        {
            var tenants = dbContext.AuthorizedTenants.Where(x => x.TenantId == id).ToList();
            dbContext.RemoveRange(tenants);
            dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult UnauthorizedTenant()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}