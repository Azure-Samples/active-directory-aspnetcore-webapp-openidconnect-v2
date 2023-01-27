using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.Graph.Constants;

namespace TodoListBFF.Controllers;

[Route("api/[controller]")]
public class AuthController : Controller
{
    [HttpGet("login")]
    public ActionResult Login(string? postLoginRedirectUri, string? claimsChallenge)
    {
        string redirectUri = !string.IsNullOrEmpty(postLoginRedirectUri) ? HttpUtility
            .UrlDecode(postLoginRedirectUri) : "/";

        var props = new AuthenticationProperties { RedirectUri = redirectUri };

        if (claimsChallenge != null)
        {
            string jsonString = claimsChallenge
                .Replace("\\", "")
                .Trim(new char[1] { '"' });

            string? loginHint = (this.User.Identity as ClaimsIdentity)?.Claims
                .FirstOrDefault(c => c.Type == "login_hint")?.Value;

            props.Items["claims"] = jsonString;
            props.Items["login_hint"] = loginHint;
        }

        return Challenge(props);
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        var props = new AuthenticationProperties { RedirectUri = "/" };

        List<string> optionList = new List<string> { 
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme 
        };

        return new SignOutResult(optionList, props);
    }

    [HttpGet("account")]
    public ActionResult GetAccount()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var claims = ((ClaimsIdentity)this.User.Identity).Claims
                .Select(c => new { type = c.Type, value = c.Value })
                .ToArray();

            return Json(new { isAuthenticated = true, claims = claims });
        }

        return Json(new { isAuthenticated = false });
    }
}
