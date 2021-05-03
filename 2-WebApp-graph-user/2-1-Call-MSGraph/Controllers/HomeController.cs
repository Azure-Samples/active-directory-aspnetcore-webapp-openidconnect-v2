using _2_1_Call_MSGraph.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace _2_1_Call_MSGraph.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly GraphServiceClient _graphServiceClient;

        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public HomeController(ILogger<HomeController> logger,
                          GraphServiceClient graphServiceClient,
                          MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            this._consentHandler = consentHandler;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public IActionResult Index()
        {
            ViewData["ApiResult"] = HttpContext.User.Identity.Name;

            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            Microsoft.Graph.User currentUser = null;

            try
            {
                currentUser = await _graphServiceClient.Me.Request().GetAsync();
            }
            catch (System.Exception ex)
            {
                if (ex is WebApiMsalUiRequiredException || (ex is ServiceException && ex.Message.Trim().Contains("Continuous access evaluation resulted in claims challenge")))
                {
                    if (ex is WebApiMsalUiRequiredException)
                    {
                        try
                        {
                            WebApiMsalUiRequiredException hex = ex as WebApiMsalUiRequiredException;
                            Console.WriteLine($"{hex}");

                            var claimChallenge = AuthenticationHeaderHelper.ExtractHeaderValues(hex);
                            _consentHandler.ChallengeUser(new string[] { "user.read" }, claimChallenge);

                            return new EmptyResult();
                        }
                        catch (Exception ex2)
                        {
                            _consentHandler.HandleException(ex2);
                        }
                    }

                    if (ex is ServiceException)
                    {
                        try
                        {
                            ServiceException svcex = ex as ServiceException;
                            Console.WriteLine($"{svcex}");
                            var claimChallenge = AuthenticationHeaderHelper.ExtractHeaderValues(svcex.ResponseHeaders);
                            _consentHandler.ChallengeUser(new string[] { "user.read" }, claimChallenge);
                            return new EmptyResult();
                        }
                        catch (Exception ex2)
                        {
                            _consentHandler.HandleException(ex2);
                        }
                    }
                }


                try
                {
                    // Get user photo
                    using (var photoStream = await _graphServiceClient.Me.Photo.Content.Request().GetAsync())
                    {
                        byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                        ViewData["Photo"] = Convert.ToBase64String(photoByte);
                    }
                }
                catch (Exception pex)
                {
                    Console.WriteLine($"{pex}");
                    ViewData["Photo"] = null;
                }
               
            }
            ViewData["Me"] = currentUser;
            return View();
        }

        public IActionResult Privacy()
        {
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