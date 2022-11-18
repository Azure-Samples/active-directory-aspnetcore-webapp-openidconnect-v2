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

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly WebOptions webOptions;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly GraphServiceClient _graphServiceClient;
        private string[] _graphScopes;

        public HomeController(
            IOptions<WebOptions> webOptionValue,
            GraphServiceClient graphServiceClient,
            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler
            )
        {
            webOptions = webOptionValue.Value;
            _consentHandler = consentHandler;
            _graphServiceClient = graphServiceClient;
            _graphScopes = webOptions.Scopes?.Split(' ');
        }

        public IActionResult Index()
        {
            ViewData["User"] = HttpContext.User;
            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                ViewData["Me"] = await _graphServiceClient.Me.Request().GetAsync();
            }

            catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge")) // CAE challenge occurred
            {
                try
                {
                    // Get challenge from response of Graph API
                    var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);

                    _consentHandler.ChallengeUser(_graphScopes, claimChallenge);

                    //restart the controller and create new GraphAPI client
                    return new EmptyResult();
                }
                catch (Exception ex2)
                {
                    _consentHandler.HandleException(ex2);
                }
            }

            ViewData["Photo"] = await GetGraphUserPhoto(_graphServiceClient);

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
            var users = await _graphServiceClient.Users.Request().GetAsync();
            ViewData["Users"] = users.CurrentPage;

            return View();
        }

        private async Task<string> GetGraphUserPhoto(GraphServiceClient graphServiceClient)
        {
            try
            {
                // Get user photo
                var photoStream = await graphServiceClient.Me.Photo.Content.Request().GetAsync();
                byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                return Convert.ToBase64String(photoByte);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}