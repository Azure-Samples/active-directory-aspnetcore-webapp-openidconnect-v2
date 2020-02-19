/*
 The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

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
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.DAL;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [AllowAnonymous]
    public class OnboardingController : Controller
    {
        private readonly SampleDbContext dbContext;
        private readonly MicrosoftIdentityOptions microsoftIdentityOptions;
        private readonly IConfiguration configuration;

        public OnboardingController(SampleDbContext dbContext, IOptions<MicrosoftIdentityOptions> microsoftIdentityOptions, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.microsoftIdentityOptions = microsoftIdentityOptions.Value;
            this.configuration = configuration;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        /// <summary>This action builds the admin consent Url to let the tenant admin consent and provision a service principal of their app in their tenant.</summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Onboard()
        {
            // Generate a random value to identify the request
            string stateMarker = Guid.NewGuid().ToString();

            AuthorizedTenant authorizedTenant = new AuthorizedTenant
            {
                CreatedOn = DateTime.Now,
                TempAuthorizationCode = stateMarker //Use the stateMarker as a tempCode, so we can locate this entity in the ProcessCode method
            };

            // Saving a temporary tenant to validate the stateMarker on the admin consent response
            dbContext.AuthorizedTenants.Add(authorizedTenant);
            dbContext.SaveChanges();

            string currentUri = UriHelper.BuildAbsolute(
                this.Request.Scheme,
                this.Request.Host,
                this.Request.PathBase);

            // Create an OAuth2 request, using the web app as the client. This will trigger a consent flow that will provision the app in the target tenant.
            // Refer to https://docs.microsoft.com/azure/active-directory/develop/v2-admin-consent for details about the Url format being constructed below
            string authorizationRequest = string.Format(
                "{0}organizations/v2.0/adminconsent?client_id={1}&redirect_uri={2}&state={3}&scope={4}",
                microsoftIdentityOptions.Instance,
                Uri.EscapeDataString(microsoftIdentityOptions.ClientId),                  // The application Id as obtained from the Azure Portal
                Uri.EscapeDataString(currentUri + "Onboarding/ProcessCode"),    // Uri that the admin will be redirected to after the consent
                Uri.EscapeDataString(stateMarker),                              // The state parameter is used to validate the response, preventing a man-in-the-middle attack, and it will also be used to identify this request in the ProcessCode action.
                Uri.EscapeDataString(configuration.GetValue<string>("GraphAPI:StaticScope")));  // The scopes to be presented to the admin to consent. Here we are using the static scope '/.default' (https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#the-default-scope).

            return Redirect(authorizationRequest);
        }

        /// <summary>
        /// This handler is used to process the response after the admin consent process is complete.
        /// </summary>
        /// <param name="tenant">The directory tenant that granted your application the permissions it requested, in GUID format..</param>
        /// <param name="error">An error code string that can be used to classify types of errors that occur, and can be used to react to errors..</param>
        /// <param name="error_description">A specific error message that can help a developer identify the root cause of an error..</param>
        /// <param name="admin_consent">Will be set to True to indicate that this response occurred on an admin consent flow..</param>
        /// <param name="state">A value included in the request that also will be returned in the token response. It can be a string of any content you want. The state is used to encode information about the user's state in the app before the authentication request occurred, such as the page or view they were on..</param>
        /// <remarks>Refer to https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-admin-consent for details on the response</remarks>
        /// <returns></returns>
        public async Task<IActionResult> ProcessCode(string tenant, string error, string error_description, bool admin_consent, string state)
        {
            if (error != null)
            {
                TempData["ErrorMessage"] = error_description;
                return RedirectToAction("Error", "Home");
            }

            if (!admin_consent)
            {
                TempData["ErrorMessage"] = "The admin consent operation failed.";
                return RedirectToAction("Error", "Home");
            }

            var authenticationProperties = new AuthenticationProperties { RedirectUri = "Home/Index" };

            // If tenant is already authorized, there is no need updated its record
            if (dbContext.AuthorizedTenants.FirstOrDefault(x => x.TenantId == tenant) != null)
            {
                // Create a Sign-in challenge to re-authenticate the user again as we need claims from the user's id_token.
                // Since the user will have a session on AAD already, they wont need to select an account again.
                return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
            }

            // Find a tenant record matching the TempAuthorizationCode that we previously saved in the Onboard()
            var preAuthorizedTenant = dbContext.AuthorizedTenants.FirstOrDefault(a => a.TempAuthorizationCode == state);

            // If we don't find it, return an error because the state param was not generated from this app and we do not wish to process this request
            if (preAuthorizedTenant == null)
            {
                TempData["ErrorMessage"] = "State verification failed.";
                return RedirectToAction("Error", "Home");
            }
            else
            {
                // Update the authorized tenant with its Id
                preAuthorizedTenant.TenantId = tenant;
                preAuthorizedTenant.AuthorizedOn = DateTime.Now;

                await dbContext.SaveChangesAsync();

                // Create a Sign-in challenge to re-authenticate the user again as we need claims from the user's id_token.
                // Since the user will have a session on AAD already, they wont need to select an account again
                return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
            }
        }
    }
}