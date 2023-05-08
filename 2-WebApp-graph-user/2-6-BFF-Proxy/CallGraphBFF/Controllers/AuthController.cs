using System.Web;
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
    public ActionResult Login(string? postLoginRedirectUri)
    {
        string redirectUri = !string.IsNullOrEmpty(postLoginRedirectUri) ? HttpUtility
            .UrlDecode(postLoginRedirectUri) : "/";

        string claims = HttpContext.Session.GetString("claimsChallenge") ?? "";

        var props = new AuthenticationProperties { RedirectUri = redirectUri };

        if (!string.IsNullOrEmpty(claims))
        {
            props.Items["claims"] = claims; // attach the challenge to the request
            HttpContext.Session.Remove("claimsChallenge"); // discard the challenge in session
        }

        return Challenge(props);
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<ActionResult> Logout(string? postLogoutRedirectUri)
    {
        string redirectUri = !string.IsNullOrEmpty(postLogoutRedirectUri) ? HttpUtility
            .UrlDecode(postLogoutRedirectUri) : "/";

        var props = new AuthenticationProperties { RedirectUri = redirectUri };

        // Sign out from both cookie and OIDC authentication schemes
        List<string> optionList = new List<string> { 
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme 
        };

        await HttpContext.SignOutAsync();
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
