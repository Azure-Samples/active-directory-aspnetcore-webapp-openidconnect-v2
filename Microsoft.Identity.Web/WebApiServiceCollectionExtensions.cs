// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
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
    /// <summary>
    /// Extensions for IServiceCollection for startup initialization of Web APIs.
    /// </summary>
    public static class WebApiServiceCollectionExtensions
    {
        /// <summary>
        /// Protects the Web API with Microsoft identity platform (formerly Azure AD v2.0)
        /// This method expects the configuration file will have a section named "AzureAd" with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="services">Service collection to which to add this authentication scheme</param>
        /// <param name="configuration">The Configuration object</param>
        /// <param name="subscribeToJwtBearerMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the JwtBearer events.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddProtectedWebApi(
            this IServiceCollection services,
            IConfiguration configuration,
            X509Certificate2 tokenDecryptionCertificate = null,
            string configSectionName = "AzureAd",
            bool subscribeToJwtBearerMiddlewareDiagnosticsEvents = false)
        {
            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                    .AddAzureADBearer(options => configuration.Bind(configSectionName, options));
            services.Configure<AzureADOptions>(options => configuration.Bind(configSectionName, options));

            services.AddHttpContextAccessor();

            // Change the authentication configuration to accommodate the Microsoft identity platform endpoint (v2.0).
            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // This is an Microsoft identity platform Web API
                options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
                options.TokenValidationParameters.ValidAudiences = new string[]
                {
                    options.Audience, $"api://{options.Audience}"
                };

                // Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                // we inject our own multi-tenant validation logic (which even accepts both v1.0 and v2.0 tokens)
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;

                // If you provide a token decryption certificate, it will be used to decrypt the token
                if (tokenDecryptionCertificate != null)
                {
                    options.TokenValidationParameters.TokenDecryptionKey = new X509SecurityKey(tokenDecryptionCertificate);
                }

                // When an access token for our own Web API is validated, we add it to MSAL.NET's cache so that it can
                // be used from the controllers.
                options.Events = new JwtBearerEvents();

                options.Events.OnTokenValidated = async context =>
                   {
                       // This check is required to ensure that the Web API only accepts tokens from tenants where it has been consented and provisioned.
                       if (!context.Principal.Claims.Any(x => x.Type == ClaimConstants.Scope)
                        && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Scp)
                        && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Roles))
                       {
                           throw new UnauthorizedAccessException("Neither scope or roles claim was found in the bearer token.");
                       }

                       await Task.FromResult(0);
                   };

                if (subscribeToJwtBearerMiddlewareDiagnosticsEvents)
                {
                    options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);
                }
            });

            return services;
        }

        // TODO: pass an option with a section name to bind the options ? or a delegate?

        /// <summary>
        /// Protects the Web API with Microsoft identity platform (formerly Azure AD v2.0)
        /// This supposes that the configuration files have a section named configSectionName (typically "AzureAD")
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="scopes">Optional parameters. If not specified, the token used to call the protected API
        /// will be kept with the user's claims until the API calls a downstream API. Otherwise the account for the
        /// user is immediately added to the token cache</param>
        /// <returns></returns>
        public static IServiceCollection AddProtectedApiCallsWebApis(
            this IServiceCollection services,
            IConfiguration configuration,
            IEnumerable<string> scopes = null,
            string configSectionName = "AzureAd")
        {
            services.AddTokenAcquisition();
            services.AddHttpContextAccessor();
            services.Configure<ConfidentialClientApplicationOptions>(options => configuration.Bind(configSectionName, options));
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
                        await tokenAcquisition.AddAccountToCacheFromJwtAsync(context, scopes).ConfigureAwait(false);
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
                    await Task.FromResult(0).ConfigureAwait(false);
                };
            });

            return services;
        }
    }
}