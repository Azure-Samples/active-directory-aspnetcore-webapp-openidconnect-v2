using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Interfaces;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly         ITokenAcquisition   tokenAcquisition;
        private readonly IGraphApiOperations graphApiOperations;

        public HomeController(ITokenAcquisition   tokenAcquisition,
                              IGraphApiOperations graphApiOperations)
        {
            this.tokenAcquisition   = tokenAcquisition;
            this.graphApiOperations = graphApiOperations;
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
            var me = await graphApiOperations.CallOnBehalfOfUserAsync(accessToken);

            ViewData["Me"] = me;
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