using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Client;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Interfaces;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ITokenAcquisition   tokenAcquisition;
        private readonly IGraphApiOperations graphApiOperations;
        private readonly IArmOperations armOperations;

        public HomeController(ITokenAcquisition   tokenAcquisition,
                              IGraphApiOperations graphApiOperations,
                              IArmOperations armOperations)
        {
            this.tokenAcquisition   = tokenAcquisition;
            this.graphApiOperations = graphApiOperations;
            this.armOperations = armOperations;
        }

        public IActionResult Index()
        {
            return View();
        }

        [MsalUiRequiredExceptionFilter(Scopes = new[] {Constants.ScopeUserRead})]
        public async Task<IActionResult> Profile()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, new[] {Constants.ScopeUserRead});

            var me = await graphApiOperations.GetUserInformation(accessToken);
            var photo = await graphApiOperations.GetPhotoAsBase64Async(accessToken);

            ViewData["Me"] = me;
            ViewData["Photo"] = photo;

            return View();
        }

        // Requires that the app has added the Azure Service Management / user_impersonation scope, and that
        // the admin tenant does not require admin consent for ARM.
        [MsalUiRequiredExceptionFilter(Scopes = new[] { "https://management.azure.com/user_impersonation" })]
        public async Task<IActionResult> Tenants()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, new[] { $"{ArmApiOperationService.ArmResource}.default" });

            var tenantIds = await armOperations.EnumerateTenantsIdsAccessibleByUser(accessToken);

            ViewData["tenants"] = new List<string>(tenantIds);

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}