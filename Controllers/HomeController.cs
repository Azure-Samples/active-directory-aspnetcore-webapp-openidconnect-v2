using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TodoListService.Extensions;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public HomeController(ITokenAcquisition tokenAcquisition)
        {
            this.tokenAcquisition = tokenAcquisition;
        }
        ITokenAcquisition tokenAcquisition;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public async Task<IActionResult> Contact()
        {
            string[] scopes = new string[] { "user.read" };

            var claims = User.Claims.ToArray();
            try

            {
                string accessToken = await tokenAcquisition.GetAccessTokenOnBehalfOfUser(User, scopes);
                dynamic me = await CallGraphApiOnBehalfOfUser(accessToken);

                ViewData["Me"] = me;
                return View();
            }
            catch(MsalException ex)
            {
                var redirectUrl = Url.Action(nameof(HomeController.Contact), "Home");
                return Challenge(
                    new AuthenticationProperties { RedirectUri = redirectUrl, IsPersistent = true },
                    OpenIdConnectDefaults.AuthenticationScheme);
            }
        }

        private static async Task<dynamic> CallGraphApiOnBehalfOfUser(string accessToken)
        {
            //
            // Call the Graph API and retrieve the user's profile.
            //
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await client.GetAsync("https://graph.microsoft.com/Beta/me");
            string content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic me = JsonConvert.DeserializeObject(content);
                return me;
            }
            else
            {
                throw new Exception(content);
            }
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
