using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Client;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly         ITokenAcquisition   tokenAcquisition;

        public HomeController(ITokenAcquisition   tokenAcquisition)
        {
            this.tokenAcquisition   = tokenAcquisition;
        }

        public IActionResult Index()
        {
            return View();
        }

        [MsalUiRequiredExceptionFilter(Scopes = new[] {Constants.ScopeUserRead})]
        public async Task<IActionResult> Profile()
        {
            // Initialize the GraphServiceClient.   
            var graphClient = await GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await tokenAcquisition.GetAccessTokenOnBehalfOfUser(
                       HttpContext, new[] { Constants.ScopeUserRead });
                return result;
            });

            // Get user profile info.
            var me = await graphClient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                // Get user photo
                var photoStream = await graphClient.Me.Photo.Content.Request().GetAsync();
                byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                ViewData["Photo"] = Convert.ToBase64String(photoByte);
            }
            catch (System.Exception)
            {
                ViewData["Photo"] = null;
            }                       

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