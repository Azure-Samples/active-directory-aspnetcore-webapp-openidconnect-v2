using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class AuthController : Controller
    {
        private readonly AzureAdB2COptions B2COptions;

        public AuthController(IOptions<AzureAdB2COptions> options)
        {
            B2COptions = options.Value;
        }

        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, B2COptions.SignUpSignInPolicy);
        }

        [HttpPost]
        public async Task SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var scheme = User.Claims.First(x => x.Type == "tfp").Value;
            await HttpContext.SignOutAsync(scheme);
        }
    }
}