/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

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