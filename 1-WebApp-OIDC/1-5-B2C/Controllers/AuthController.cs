using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class AuthController : Controller
    {
        private readonly AzureADB2COptions B2COptions;

        public AuthController(IOptions<AzureADB2COptions> options)
        {
            B2COptions = options.Value;
        }

        public IActionResult SignIn()
        {
            /* 
             This Challenge will trigger the OpenIdConnect middleware configured on Startup.cs. 
             Then we redirect to the homepage, otherwise we would be redirected this action result again and enter in a loop
            */
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, B2COptions.SignUpSignInPolicyId);
        }

        /*
         We are using POST to sign-out because Chrome for example, prefetches pages to speed up browsing.
         And prefetching the sign out page would cause the user to sign out without reason, so using a form gets us around that.
         */
        [HttpPost]
        public async Task SignOut()
        {
            // Signs-out the user from cookie scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var scheme = User.Claims.First(x => x.Type == "tfp").Value;

            // Signs-out the user from any currently active authentication session in Azure AD with the policy scheme in use
            await HttpContext.SignOutAsync(scheme);
        }
    }
}