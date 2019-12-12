using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public IActionResult SignIn()
        {
            var redirectUrl = Url.Content("~/");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl });
        }

        public IActionResult SignOut()
        {
            var callbackUrl = Url.Page("/Account/SignedOut", pageHandler: null, values: null, protocol: Request.Scheme);
            return SignOut(
                 new AuthenticationProperties
                 {
                     RedirectUri = "/Account/SignedOut"
                 },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult SignedOut() => View();
    }
}