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
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GraphHelper _graphHelper;

        public HomeController(IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            string[] graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            _httpContextAccessor = httpContextAccessor;

            if (this._httpContextAccessor.HttpContext != null)
            {
                this._graphHelper = new GraphHelper(this._httpContextAccessor.HttpContext, graphScopes);
            }
        }

        public IActionResult Index()
        {
            ViewData["User"] = _httpContextAccessor.HttpContext.User;
            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            ViewData["Me"] = await _graphHelper.GetMeAsync();

            if (ViewData["Me"] == null)
            {
                return new EmptyResult();
            }

            var photoStream = await this._graphHelper.GetMyPhotoAsync();
            ViewData["Photo"] = photoStream != null ? Convert.ToBase64String(((MemoryStream)photoStream).ToArray()) : null;

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
            ViewData["Users"] = await this._graphHelper.GetUsersAsync();

            if (ViewData["Users"] == null)
            {
                return new EmptyResult();
            }

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