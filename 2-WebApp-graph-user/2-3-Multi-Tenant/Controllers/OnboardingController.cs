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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApp_OpenIDConnect_DotNet.DAL;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [AllowAnonymous]
    public class OnboardingController : Controller
    {
        private readonly SampleDbContext dbContext;
        private readonly AzureADOptions azureADOptions;

        public OnboardingController(SampleDbContext dbContext, IOptions<AzureADOptions> azureADOptions)
        {
            this.dbContext = dbContext;
            this.azureADOptions = azureADOptions.Value;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Onboard()
        {
            // Generate a random value to identify the request
            string stateMarker = Guid.NewGuid().ToString();

            AuthorizedTenant authorizedTenant = new AuthorizedTenant
            {
                CreatedOn = DateTime.Now,
                TempAuthorizationCode = stateMarker //Use the stateMarker as a tempCode, so we can find this entity on the ProcessCode method
            };

            dbContext.AuthorizedTenants.Add(authorizedTenant);
            dbContext.SaveChanges();

            string currentUri = UriHelper.BuildAbsolute(
                this.Request.Scheme,
                this.Request.Host,
                this.Request.PathBase);

            // Create an OAuth2 request, using the web app as the client.This will trigger a consent flow that will provision the app in the target tenant.
            string authorizationRequest = string.Format(
                "{0}common/v2.0/adminconsent?client_id={1}&redirect_uri={2}&state={3}&scope={4}",
                azureADOptions.Instance,
                Uri.EscapeDataString(azureADOptions.ClientId),
                Uri.EscapeDataString(currentUri + "Onboarding/ProcessCode"),
                Uri.EscapeDataString(stateMarker),
                Uri.EscapeDataString("https://graph.microsoft.com/.default"));


            return Redirect(authorizationRequest);
        }

        // This is the redirect Uri for the admin consent authorization
        public async Task<IActionResult> ProcessCode(string tenant, string error, string error_description, string resource, string state)
        {
            if (error != null)
            {
                TempData["ErrorMessage"] = error_description;
                return RedirectToAction("Error", "Home");
            }

            // Check if tenant is already authorized
            if(dbContext.AuthorizedTenants.FirstOrDefault(x => x.TenantId == tenant) != null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Find a tenant carrying a TempAuthorizationCode that we previously saved
            var preAuthorizedTenant = dbContext.AuthorizedTenants.FirstOrDefault(a => a.TempAuthorizationCode == state);

            // If we don't find it, return an error because the state param was not generated from this app
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

                return RedirectToAction("Index", "Home");
            }
        }
    }
}