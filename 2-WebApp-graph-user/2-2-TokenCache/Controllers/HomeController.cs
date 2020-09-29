using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Models;
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

        [AuthorizeForScopes(Scopes = new[] { Infrastructure.Constants.ScopeUserRead})]
        public async Task<IActionResult> Profile()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(new[] { Infrastructure.Constants.ScopeUserRead});

            var me = await graphApiOperations.GetUserInformation(accessToken);
            var photo = await graphApiOperations.GetPhotoAsBase64Async(accessToken);

            ViewData["Me"] = me;
            ViewData["Photo"] = photo;

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