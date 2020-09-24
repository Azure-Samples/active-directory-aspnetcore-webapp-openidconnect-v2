using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class AccountController : Controller
    {
        [Authorize]
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("SignOut", "Account", new { area = "MicrosoftIdentity" });
        }
    }
}
