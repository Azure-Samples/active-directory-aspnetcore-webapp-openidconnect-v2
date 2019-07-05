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
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web.Client;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    public static class WebApiStartupHelpers
    {
        /// <summary>
        /// Protects the Web API with Microsoft Identity Platform v2.0 (AAD v2.0)
        /// This expects the configuration files will have a section named "AzureAD"
        /// </summary>
        /// <param name="services">Service collection to which to add this authentication scheme</param>
        /// <param name="configuration">The Configuration object</param>
        /// <returns></returns>
        public static IServiceCollection AddProtectWebApiWithMicrosoftIdentityPlatformV2(this IServiceCollection services, IConfiguration configuration, X509Certificate2 tokenDecryptionCertificate = null)
        {
            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                    .AddAzureADBearer(options => configuration.Bind("AzureAd", options));

            // Add session if you are planning to use session based token cache , .AddSessionTokenCaches()
            services.AddSession();

            // Change the authentication configuration  to accommodate the Azure AD v2.0 endpoint.
            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // Reinitialize the options as this has changed to JwtBearerOptions to pick configuration values for attributes unique to JwtBearerOptions
                configuration.Bind("AzureAd", options);

                // This is an Azure AD v2.0 Web API
                options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
                options.TokenValidationParameters.ValidAudiences = new string[] { options.Audience, $"api://{options.Audience}" };

                // Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                // we inject our own multi-tenant validation logic (which even accepts both V1 and V2 tokens)
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;

                // If you provide a token decryption certificate, it will be used to decrypt the token
                if (tokenDecryptionCertificate != null)
                {
                    options.TokenValidationParameters.TokenDecryptionKey = new X509SecurityKey(tokenDecryptionCertificate);
                }

                // When an access token for our own Web API is validated, we add it to MSAL.NET's cache so that it can
                // be used from the controllers.
                options.Events = new JwtBearerEvents();

#pragma warning disable 1998
                options.Events.OnTokenValidated = async context =>
                   {
                       // This check is required to ensure that the Web API only accepts tokens from tenants where it has been consented and provisioned.
                       if (!context.Principal.Claims.Any(x => x.Type == ClaimConstants.Scope)
                          && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Roles))
                       {
                           throw new UnauthorizedAccessException("Neither scope or roles claim was found in the bearer token.");
                       }

                       return;
                   };
#pragma warning restore 1998

                // If you want to debug, or just understand the JwtBearer events, uncomment the following line of code
                // options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);
            });

            return services;
        }

        /// <summary>
        /// Protects the Web API with Microsoft Identity Platform v2.0 (AAD v2.0)
        /// This supposes that the configuration files have a section named "AzureAD"
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="scopes">Optional parameters. If not specified, the token used to call the protected API
        /// will be kept with the user's claims until the API calls a downstream API. Otherwise the account for the
        /// user is immediately added to the token cache</param>
        /// <returns></returns>
        public static IServiceCollection AddProtectedApiCallsWebApis(this IServiceCollection services, IConfiguration configuration, IEnumerable<string> scopes = null)
        {
            services.AddTokenAcquisition();
            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // If you don't pre-provide scopes when adding calling AddProtectedApiCallsWebApis, the On behalf of
                // flow will be delayed (lazy construction of MSAL's application
                options.Events.OnTokenValidated = async context =>
                {
                    if (scopes != null && scopes.Any())
                    {
                        var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                        context.Success();
                        tokenAcquisition.AddAccountToCacheFromJwt(context, scopes);
                    }
                    else
                    {
                        context.Success();

                        // Todo : rather use options.SaveToken?
                        JwtSecurityToken jwtSecurityToken = context.SecurityToken as JwtSecurityToken;
                        if (jwtSecurityToken != null)
                        {
                            string rawData = (jwtSecurityToken.InnerToken != null) ? jwtSecurityToken.InnerToken.RawData : jwtSecurityToken.RawData;
                            (context.Principal.Identity as ClaimsIdentity).AddClaim(new Claim("jwt", rawData));
                        }
                    }
                    // Adds the token to the cache, and also handles the incremental consent and claim challenges
                    await Task.FromResult(0);
                };
            });

            return services;
        }
    }
}