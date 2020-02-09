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
                // Reinitialize the options as this has changed to JwtBearerOptions to pick configuration values for attributes unique to JwtBearerOptions
                configuration.Bind(configSectionName, options);

                // This is an Microsoft identity platform Web API
                EnsureAuthorityIsV2_0(options);

                // The valid audience could be given as Client Id or as Uri. 
                // If it does not start with 'api://', this variant is added to the list of valid audiences.
                EnsureValidAudiencesContainsApiGuidIfGuidProvided(options);

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
        /// <returns></returns>
        public static IServiceCollection AddProtectedApiCallsWebApis(
            this IServiceCollection services,
            IConfiguration configuration,
            string configSectionName = "AzureAd")
        {
            services.AddTokenAcquisition();
            services.AddHttpContextAccessor();
            services.Configure<ConfidentialClientApplicationOptions>(options => configuration.Bind(configSectionName, options));
            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                options.Events.OnTokenValidated = async context =>
                {
                    context.HttpContext.StoreTokenUsedToCallWebAPI(context.SecurityToken as JwtSecurityToken);
                    context.Success();
                    await Task.FromResult(0).ConfigureAwait(false);
                };
            });

            return services;
        }

        /// <summary>
        /// Ensures that the authority is a v2.0 authority
        /// </summary>
        /// <param name="options">Jwt bearer options read from the config file
        /// or set by the developper, for which we want to ensure the authority
        /// is a v2.0 authority</param>
        internal static void EnsureAuthorityIsV2_0(JwtBearerOptions options)
        {
            var authority = options.Authority.Trim().TrimEnd('/');
            if (!authority.EndsWith("v2.0"))
                authority += "/v2.0";
            options.Authority = authority;
        }


        /// <summary>
        /// Ensure that if the audience is a GUID, api://{audience} is also added
        /// as a valid audience (this is the default App ID URL in the app registration
        /// portal)
        /// </summary>
        /// <param name="options">Jwt bearer options for which to ensure that
        /// api://GUID is a valid audience</param>
        internal static void EnsureValidAudiencesContainsApiGuidIfGuidProvided(JwtBearerOptions options)
        {
            var validAudiences = new List<string> { options.Audience };
            if (!options.Audience.StartsWith("api://", StringComparison.OrdinalIgnoreCase)
                                             && Guid.TryParse(options.Audience, out _))
                validAudiences.Add($"api://{options.Audience}");

            options.TokenValidationParameters.ValidAudiences = validAudiences;
        }
    }
}
