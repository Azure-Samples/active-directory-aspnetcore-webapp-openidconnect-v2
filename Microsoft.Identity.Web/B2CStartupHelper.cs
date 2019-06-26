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

using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.Identity.Web
{
    public static class B2CStartupHelper
    {
        /// <summary>
        /// Add authentication with Azure Ad B2C.
        /// This expects the configuration files will have a section named "AzureAdB2C"
        /// </summary>
        /// <param name="services">Service collection to which to add this authentication scheme</param>
        /// <param name="configuration">The Configuration object</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureAdB2CAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var b2cOptions = configuration.GetSection("AzureAdB2C").Get<AzureADB2COptions>();

            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = b2cOptions.SignUpSignInPolicyId;
            }).AddOpenIdConnect(b2cOptions.SignUpSignInPolicyId, options =>
            {
                options.MetadataAddress = $"{b2cOptions.Instance}/{b2cOptions.Domain}/v2.0/.well-known/openid-configuration?p={b2cOptions.SignUpSignInPolicyId}";
                options.ClientId = b2cOptions.ClientId;
                options.ResponseType = OpenIdConnectResponseType.IdToken;
                options.CallbackPath = b2cOptions.CallbackPath;
                options.SignedOutCallbackPath = b2cOptions.SignedOutCallbackPath;
                options.SignedOutRedirectUri = "/";
                options.TokenValidationParameters.NameClaimType = "name";
                options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                options.Scope.Add(OpenIdConnectScope.OfflineAccess);
            }).AddCookie(); ;

            return services;
        }
    }
}