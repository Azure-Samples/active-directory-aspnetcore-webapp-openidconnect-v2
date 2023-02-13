using System;
using Microsoft.AspNetCore.Mvc;

namespace ToDoListService.Controllers
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
            var decodeUrl = System.Web.HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString());
            var queryString = System.Web.HttpUtility.ParseQueryString(decodeUrl);
            var clientRedirect = queryString["state"];
            if (!string.IsNullOrEmpty(clientRedirect))
            {
                if (queryString["error"] == "access_denied" && queryString["error_subcode"] == "cancel")
                {
                    var clientRedirectUri = new Uri(clientRedirect);
                    return Redirect(clientRedirectUri.GetLeftPart(System.UriPartial.Authority));
                }
                else
                {
                    return Redirect(clientRedirect);
                }
            }
            else
            {
                return RedirectToAction("GetTodoItems", "TodoList");
            }
        }
    }
}