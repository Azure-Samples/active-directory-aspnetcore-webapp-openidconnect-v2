using _2_1_Call_MSGraph.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Utils;

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
            _consentHandler = consentHandler;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Index()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();
            ViewData["ApiResult"] = user.DisplayName;

            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            var me = await _graphServiceClient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                // Get user photo
                using (var photoStream = await _graphServiceClient.Me.Photo.Content.Request().GetAsync())
                {
                    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                    ViewData["Photo"] = Convert.ToBase64String(photoByte);
                }
            }
            catch (System.Exception)
            {
                ViewData["Photo"] = null;
            }

            return View();
        }

        /// <summary>
        /// Fetches a list of files from a SharePoint site
        /// </summary>
        /// <returns></returns>
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Files()
        {
            List<ListItem> files = new List<ListItem>();

            try
            {
                // We need to add this to Graph calls to get WWW-Authenticate challenge
                var headerOptions = new List<HeaderOption>()
                {
                    new HeaderOption("x-ms-cc","t")
                };

                // kkcommsite
                // var items = await _graphServiceClient.Sites["kko365.sharepoint.com,e442d570-e5ec-4620-b574-dba029d70df9,a85626c3-c071-42f9-90d5-750c54b56f85"].Lists["dc2e425d-b409-4c4b-8b9b-11288e184eb8"].Items.Request().GetAsync();

                // kkteamsite
                try
                {
                    // The Call to SharePoint via MS Graph
                    var items = await _graphServiceClient.Sites["kko365.sharepoint.com,2be53b58-1ba9-46e4-ba6a-f7e51fd60a5f,819c35fe-9009-4f4d-b7f0-ef13dd4ffebe"].Lists["2aeaf7a2-e207-4352-91fd-2e8a350323b0"].Items.Request(headerOptions).GetAsync();

                    // Pull all pages of files
                    var pageIterator = PageIterator<ListItem>.CreatePageIterator(_graphServiceClient, items, (m) =>
                    {
                        Console.WriteLine(m.WebUrl);
                        files.Add(m);
                        return true;
                    });

                    await pageIterator.IterateAsync();

                    while (pageIterator.State != PagingState.Complete)
                    {
                        await pageIterator.ResumeAsync();
                    }

                    // Print the list on the UI
                    ViewData["Files"] = files;
                    ViewData["Message"] = $"{files.Count} files from the sharepoint site";
                }
                catch (ServiceException sx)
                {
                    if (sx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        HttpResponseHeaders header = sx.ResponseHeaders;
                        WwwAuthenticateHelper wwwAuth = null;

                        // If WWW-Authenticate happened
                        if (header.WwwAuthenticate != null)
                        {
                            wwwAuth = new WwwAuthenticateHelper(header.WwwAuthenticate);
                        }

                        try
                        {
                            if (null != wwwAuth && null != wwwAuth.Claims)
                            {
                                _consentHandler.ChallengeUser(new string[] { "user.read Sites.Read.All" }, wwwAuth.Claims);
                                return new EmptyResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            _consentHandler.HandleException(ex);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ViewData["Message"] = $"{ex}";
                ViewData["Files"] = null;
            }

            ViewData["Title"] = "SharePoint graph call demo";

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