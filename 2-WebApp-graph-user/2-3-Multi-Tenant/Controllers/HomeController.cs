/*
 The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Linq;
using WebApp_OpenIDConnect_DotNet.DAL;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly SampleDbContext dbContext;

        public HomeController(SampleDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a list all authorized tenants to be displayed in a table, for demonstration purpose only
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var authorizedTenants = dbContext.AuthorizedTenants.Where(x => x.TenantId != null && x.AuthorizedOn != null).ToList();
            return View(authorizedTenants);
        }

        /// <summary>Deletes the selected tenant from the app's own DB (off-boarding).</summary>
        /// <param name="id">The tenant Id.</param>
        /// <returns></returns>
        public IActionResult DeleteTenant(string id)
        {
            var tenants = dbContext.AuthorizedTenants.Where(x => x.TenantId == id).ToList();
            dbContext.RemoveRange(tenants);
            dbContext.SaveChanges();

            var signedUsersTenant = User.GetTenantId();

            // If the user deletes its own tenant from the list, they would also be signed-out
            if (id == signedUsersTenant)
                return RedirectToAction("SignOut", "Account", new { area = "MicrosoftIdentity" });
            else
                return RedirectToAction("Index");
        }

        /// <summary>
        /// If you landed here, its because you tried to sign-in with a user account from a tenant that hasn't onboarded the application yet.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult UnauthorizedTenant()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}