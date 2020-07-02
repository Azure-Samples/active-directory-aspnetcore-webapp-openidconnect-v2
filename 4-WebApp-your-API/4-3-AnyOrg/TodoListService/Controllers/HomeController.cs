using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TodoListService.Controllers
{
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class HomeController : Controller
    {
        /// <summary>
        /// Landing action after service principal is provisioned for Web API.
        /// Gets value from state and redirects back to Web App.
        /// </summary>
        /// <returns></returns>
        public IActionResult AdminConsent()
        {
            var queryString = System.Web.HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
            var clientRedirect = queryString["state"];
            return Redirect(clientRedirect);
        }
    }
}