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
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly WebOptions webOptions;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public HomeController(ITokenAcquisition tokenAcquisition, IOptions<WebOptions> webOptionValue, MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.webOptions = webOptionValue.Value;
            _consentHandler = consentHandler;
        }

        public IActionResult Index()
        {
            ViewData["User"] = HttpContext.User;
            return View();
        }

        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]
        public async Task<IActionResult> Profile()
        {
            GraphServiceClient graphClient;

            try
            {
                // Initialize the GraphServiceClient.
                graphClient = GetGraphServiceClient(new[] { Constants.ScopeUserRead });   
                ViewData["Me"] = await graphClient.Me.Request().GetAsync();    
            }

            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge")) // CAE challenge occurred
            {
                graphClient = GetGraphServiceClientPostCAE(new[] { Constants.ScopeUserRead }, ex.ResponseHeaders);
                ViewData["Me"] = await graphClient.Me.Request().GetAsync();
            }
           
            ViewData["Photo"] = await GetGraphUserPhoto(graphClient);

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

        private GraphServiceClient GetGraphServiceClientPostCAE(string[] scopes, HttpResponseHeaders headers)
        {
            // Get challenge from response of Graph API
            var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(headers);

            _consentHandler.ChallengeUser(scopes, claimChallenge);

            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                return result;
            }, webOptions.GraphApiUrl);
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
            catch (Exception)
            {
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