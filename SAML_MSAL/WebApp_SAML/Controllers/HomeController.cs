using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApp_SAML.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var app = PublicClientApplicationBuilder.Create("992f2eec-20ec-4eb4-952b-2d974f0db6ea")
                .WithAuthority("https://login.microsoftonline.com/979f4440-75dc-4664-b2e1-2cafa0ac67d1/v2.0")
                .Build();

            // Set public client true
            // Grant consent via portal

            var securePassword = new SecureString();
            foreach (char c in "Vancouver1")        
                securePassword.AppendChar(c);
            
            var result = await app.AcquireTokenByUsernamePassword(new string[] { "User.Read" }, "test1@lab.cxpaadtenant.com", securePassword)
                .ExecuteAsync().ConfigureAwait(false);

            var userClaims = ClaimsPrincipal.Current.Claims.ToList();
            return View(userClaims);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}