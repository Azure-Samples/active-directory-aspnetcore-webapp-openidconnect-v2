using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services;
using Graph = Microsoft.Graph;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly WebOptions webOptions;

        public HomeController(ITokenAcquisition tokenAcquisition, IOptions<WebOptions> webOptionValue)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.webOptions = webOptionValue.Value;
        }

        public IActionResult Index()
        {
            ViewData["User"] = HttpContext.User;
            return View();
        }

        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]
        public async Task<IActionResult> Profile()
        {
            // Initialize the GraphServiceClient.
            Graph::GraphServiceClient graphClient = GetGraphServiceClient(new[] { Constants.ScopeUserRead });

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

        /// <summary>
        /// Fetches and displays all the users in this directory. This method requires the signed-in user to be assigned to the 'UserReaders' approle.
        /// </summary>
        /// <returns></returns>
        [AuthorizeForScopes(Scopes = new[] { GraphScopes.UserReadBasicAll })]
        [Authorize(Policy = AuthorizationPolicies.AssignmentToUserReaderRoleRequired)]
        public async Task<IActionResult> Users()
        {
            // Initialize the GraphServiceClient.
            Graph::GraphServiceClient graphClient = GetGraphServiceClient(new[] { GraphScopes.UserReadBasicAll });

            var users = await graphClient.Users.Request().GetAsync();
            ViewData["Users"] = users.CurrentPage;

            return View();
        }

        private Graph::GraphServiceClient GetGraphServiceClient(string[] scopes)
        {
            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                return result;
            }, webOptions.GraphApiUrl);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}