using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using ToDoListClient.Models;

namespace ToDoListClient.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;

        private readonly string _TodoListScope = string.Empty;
        private readonly string _ClientId = string.Empty;
        private readonly string _RedirectUri = string.Empty;
        private readonly string _ApiClientId = string.Empty;
        private readonly string _ApiRedirectUri = string.Empty;
        private readonly string _ApiScope = "https://graph.microsoft.com/.default";

        public HomeController(ITokenAcquisition tokenAcquisition, IConfiguration configuration)
        {
            this.tokenAcquisition = tokenAcquisition;
            _TodoListScope = configuration["TodoList:TodoListScope"];
            _ClientId = configuration["AzureAd:ClientId"];
            _RedirectUri = configuration["RedirectUri"];
            _ApiClientId = configuration["TodoList:TodoListAppId"];
            _ApiRedirectUri = configuration["TodoList:AdminConsentRedirectApi"];
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
        /// Creates Admin Consent Endpoint and redirects to the endpoint for Web API provisioning.
        /// Admin consent has a URL in state and it is used to redirect back to Web App once SP for API is provisioned.
        /// </summary>
        /// <returns></returns>
        public IActionResult AdminConsentApi()
        {
            string adminConsent1 = "https://login.microsoftonline.com/common/v2.0/adminconsent?client_id=" + _ApiClientId
                + "&redirect_uri=" + _ApiRedirectUri
                + "&state=" + _RedirectUri + "Home/AdminConsentClient" + "&scope=" + _ApiScope;

            return Redirect(adminConsent1);
        }

        /// <summary>
        /// Creates Admin Consent Endpoint and redirects to the endpoint for Web App provisioning.
        /// </summary>
        /// <returns></returns>
        public IActionResult AdminConsentClient()
        {
            string adminConsent2 = "https://login.microsoftonline.com/common/v2.0/adminconsent?client_id=" + _ClientId
                + "&redirect_uri=" + _RedirectUri
                + "&state=123&scope=" + _TodoListScope;

            return Redirect(adminConsent2);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
