using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using ToDoListClient.Models;

namespace ToDoListClient.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly string _TodoListScope = string.Empty;
        private readonly string _ClientId = string.Empty;
        private readonly string _RedirectUri = string.Empty;
        private readonly string _ApiClientId = string.Empty;
        private readonly string _ApiRedirectUri = string.Empty;
        private readonly string _ApiScope = "https://graph.microsoft.com/.default";

        public HomeController(IConfiguration configuration)
        {
            _TodoListScope = configuration["TodoList:TodoListServiceScope"];
            _ClientId = configuration["AzureAd:ClientId"];
            _RedirectUri = configuration["RedirectUri"];
            _ApiClientId = configuration["TodoList:TodoListServiceAppId"];
            _ApiRedirectUri = configuration["TodoList:AdminConsentRedirectApi"];
            if (!string.IsNullOrEmpty(_RedirectUri))
            {
                if (!_RedirectUri.EndsWith("/"))
                {
                    _RedirectUri += "/";
                }
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Creates a Url to the Admin Consent Endpoint and redirects the user to the end point provisioning the Web API.
        /// Admin consent has a URL in state and it is used to redirect back to Web App once the SP for API is provisioned.
        /// </summary>
        /// <returns></returns>
        public IActionResult AdminConsentApi()
        {
            string adminConsent = "https://login.microsoftonline.com/organizations/v2.0/adminconsent?client_id=" + _ApiClientId
                + "&redirect_uri=" + _ApiRedirectUri
                + "&state=" + _RedirectUri + "Home/AdminConsentClient" + "&scope=" + _ApiScope;

            return Redirect(adminConsent);
        }

        /// <summary>
        /// Creates a Url to the Admin Consent Endpoint and redirects the user to the end point provisioning the Web app.
        /// </summary>
        /// <returns></returns>
        public IActionResult AdminConsentClient()
        {
            string adminConsent = "https://login.microsoftonline.com/organizations/v2.0/adminconsent?client_id=" + _ClientId
                + "&redirect_uri=" + _RedirectUri
                + "&state=123&scope=" + _TodoListScope;

            return Redirect(adminConsent);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}