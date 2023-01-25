using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TodoListBFF.Controllers;

[Route("api/[controller]")]
public class AuthController : Controller
{
    [HttpGet("login")]
    public ActionResult Login()
    {
        return Challenge(new AuthenticationProperties { 
            RedirectUri = "/"
        });
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        List<string> optionList = new List<string> { 
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme 
        };

        return new SignOutResult(optionList, new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }

    [HttpGet("account")]
    public ActionResult GetAccount()
    {
        if (User.Identity.IsAuthenticated)
        {
            var claims = ((ClaimsIdentity)this.User.Identity).Claims
                .Select(c => new { type = c.Type, value = c.Value })
                .ToArray();

            return Json(new { isAuthenticated = true, claims = claims });
        }

        return Json(new { isAuthenticated = false });
    }
}
