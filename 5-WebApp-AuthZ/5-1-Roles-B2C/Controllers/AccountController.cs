using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// AspNet core's default AuthorizeAttribute redirects to '/Account/AccessDenied' when it processes the Http code 403 (Unauthorized)
        /// Instead of implementing an Attribute class of our own to construct a redirect Url, we'd just implement our own to show an error message of
        /// our choice to the user.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}