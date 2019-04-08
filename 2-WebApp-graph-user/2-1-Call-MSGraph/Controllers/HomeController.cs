using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Client;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services;
using WebApp_OpenIDConnect_DotNet.Services.GraphOperations;

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
            // Initialize the GraphServiceClient.   
            var graphClient = await GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await tokenAcquisition.GetAccessTokenOnBehalfOfUser(
                       HttpContext, new[] { Constants.ScopeUserRead });
                return result;
            });

            var me = await graphClient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                var photo = await graphClient.Me.Photo.Request().GetAsync();
                ViewData["Photo"] = photo;
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