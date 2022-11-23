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
        //private readonly WebOptions webOptions;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GraphHelper _graphHelper;

        public HomeController(IHttpContextAccessor httpContextAccessor,
            GraphServiceClient graphServiceClient,
            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
            IConfiguration configuration)
        {
            _consentHandler = consentHandler;
            _graphServiceClient = graphServiceClient;
            string[] graphScopes = configuration.GetValue<string>("GraphAPI:Scopes")?.Split(' ');
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
            //try
            //{
            //    ViewData["Me"] = await _graphServiceClient.Me.Request().GetAsync();
            ViewData["Me"] = await _graphHelper.GetMeAsync();
            //}
            //catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge")) // CAE challenge occurred
            //{
            //    try
            //    {
            //        // Get challenge from response of Graph API
            //        var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);

            //        _consentHandler.ChallengeUser(_graphScopes, claimChallenge);

            //        //restart the controller and create new GraphAPI client
            //        return new EmptyResult();
            //    }
            //    catch (Exception ex2)
            //    {
            //        _consentHandler.HandleException(ex2);
            //    }
            //}

            //ViewData["Photo"] = await GetGraphUserPhoto(_graphServiceClient);
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
            //var users = await _graphServiceClient.Users.Request().GetAsync();
            ViewData["Users"] = await this._graphHelper.GetUsersAsync();

            return View();
        }

        //private async Task<string> GetGraphUserPhoto(GraphServiceClient graphServiceClient)
        //{
        //    //try
        //    //{
        //    // Get user photo
        //    //var photoStream = await graphServiceClient.Me.Photo.Content.Request().GetAsync();
        //    var photoStream = await this._graphHelper.GetMyPhotoAsync();
        //    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
        //    return Convert.ToBase64String(photoByte);
        //    //}
        //    //catch (ServiceException svcex) when (svcex.Error.Code == "ImageNotFound")
        //    //{
        //    //    Console.WriteLine($"{svcex.Message}");
        //    //    return null;
        //    //}
        //}

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}