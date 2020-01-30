// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
        #region Compatibility
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
            return AddProtectedWebApiCallsProtectedWebApi(services,
                                                          configuration,
                                                          configSectionName);
        }
        #endregion

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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddProtectedWebApi(
                    configSectionName,
                    configuration,
                    options => configuration.Bind(configSectionName, options),
                    tokenDecryptionCertificate,
                    subscribeToJwtBearerMiddlewareDiagnosticsEvents);

            return services;
        }

        /// <summary>
        /// Protects the Web API with Microsoft identity platform (formerly Azure AD v2.0)
        /// This method expects the configuration file will have a section, named "AzureAd" as default, with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="builder">AuthenticationBuilder to which to add this configuration</param>
        /// <param name="configuration">The Configuration object</param>
        /// <param name="configureOptions">An action to configure JwtBearerOptions</param>
        /// <param name="tokenDecryptionCertificate">Token decryption certificate</param>
        /// <param name="subscribeToJwtBearerMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the JwtBearer events.
        /// </param>
        /// <returns></returns>
        public static AuthenticationBuilder AddProtectedWebApi(
            this AuthenticationBuilder builder,
            IConfiguration configuration,
            Action<JwtBearerOptions> configureOptions,
            X509Certificate2 tokenDecryptionCertificate = null,
            bool subscribeToJwtBearerMiddlewareDiagnosticsEvents = false)
        {
            return AddProtectedWebApi(
                builder,
                "AzureAd",
                configuration,
                JwtBearerDefaults.AuthenticationScheme,
                configureOptions,
                tokenDecryptionCertificate,
                subscribeToJwtBearerMiddlewareDiagnosticsEvents);
        }

        /// <summary>
        /// Protects the Web API with Microsoft identity platform (formerly Azure AD v2.0)
        /// This method expects the configuration file will have a section, named "AzureAd" as default, with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="builder">AuthenticationBuilder to which to add this configuration</param>
        /// <param name="configSectionName">The configuration section with the necessary settings to initialize authentication options</param>
        /// <param name="configuration">The Configuration object</param>
        /// <param name="configureOptions">An action to configure JwtBearerOptions</param>
        /// <param name="tokenDecryptionCertificate">Token decryption certificate</param>
        /// <param name="subscribeToJwtBearerMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the JwtBearer events.
        /// </param>
        /// <returns></returns>
        public static AuthenticationBuilder AddProtectedWebApi(
            this AuthenticationBuilder builder,
            string configSectionName,
            IConfiguration configuration,
            Action<JwtBearerOptions> configureOptions,
            X509Certificate2 tokenDecryptionCertificate = null,
            bool subscribeToJwtBearerMiddlewareDiagnosticsEvents = false)
        {
            return AddProtectedWebApi(
                builder,
                configSectionName,
                configuration,
                JwtBearerDefaults.AuthenticationScheme,
                configureOptions,
                tokenDecryptionCertificate,
                subscribeToJwtBearerMiddlewareDiagnosticsEvents);
        }

        /// <summary>
        /// Protects the Web API with Microsoft identity platform (formerly Azure AD v2.0)
        /// This method expects the configuration file will have a section, named "AzureAd" as default, with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="builder">AuthenticationBuilder to which to add this configuration</param>
        /// <param name="configSectionName">The configuration section with the necessary settings to initialize authentication options</param>
        /// <param name="configuration">The Configuration object</param>
        /// <param name="jwtBearerScheme">The JwtBearer scheme name to be used. By default it uses "Bearer"</param>
        /// <param name="configureOptions">An action to configure JwtBearerOptions</param>
        /// <param name="tokenDecryptionCertificate">Token decryption certificate</param>
        /// <param name="subscribeToJwtBearerMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the JwtBearer events.
        /// </param>
        /// <returns></returns>
        public static AuthenticationBuilder AddProtectedWebApi(
            this AuthenticationBuilder builder,
            string configSectionName,
            IConfiguration configuration,
            string jwtBearerScheme,
            Action<JwtBearerOptions> configureOptions,
            X509Certificate2 tokenDecryptionCertificate = null,
            bool subscribeToJwtBearerMiddlewareDiagnosticsEvents = false)
        {
            builder.Services.Configure(jwtBearerScheme, configureOptions);
            builder.Services.Configure<MicrosoftIdentityOptions>(options => configuration.Bind(configSectionName, options));

            builder.Services.AddHttpContextAccessor();

            // Change the authentication configuration to accommodate the Microsoft identity platform endpoint (v2.0).
            builder.AddJwtBearer(jwtBearerScheme, options =>
            {
                var microsoftIdentityOptions = configuration.GetSection(configSectionName).Get<MicrosoftIdentityOptions>();

                if (string.IsNullOrWhiteSpace(options.Authority))
                    options.Authority = AuthorityHelpers.BuildAuthority(microsoftIdentityOptions);

                if (!AuthorityHelpers.IsV2Authority(options.Authority))
                    options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
                options.TokenValidationParameters.ValidAudiences = new string[]
                {
                    // If the developer doesn't set the Audience on JwtBearerOptions, use ClientId from MicrosoftIdentityOptions
                    options.Audience, $"api://{options.Audience ?? microsoftIdentityOptions.ClientId}"
                };

                // If the developer registered an IssuerValidator, do not overwrite it
                if (options.TokenValidationParameters.IssuerValidator == null)
                {
                    // Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                    // we inject our own multi-tenant validation logic (which even accepts both v1.0 and v2.0 tokens)
                    options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;
                }

                // If you provide a token decryption certificate, it will be used to decrypt the token
                if (tokenDecryptionCertificate != null)
                {
                    options.TokenValidationParameters.TokenDecryptionKey = new X509SecurityKey(tokenDecryptionCertificate);
                }

                if (options.Events == null)
                    options.Events = new JwtBearerEvents();

                // When an access token for our own Web API is validated, we add it to MSAL.NET's cache so that it can
                // be used from the controllers.
                var tokenValidatedHandler = options.Events.OnTokenValidated;
                options.Events.OnTokenValidated = async context =>
                {
                    // This check is required to ensure that the Web API only accepts tokens from tenants where it has been consented and provisioned.
                    if (!context.Principal.Claims.Any(x => x.Type == ClaimConstants.Scope)
                    && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Scp)
                    && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Roles))
                    {
                        throw new UnauthorizedAccessException("Neither scope or roles claim was found in the bearer token.");
                    }

                    await tokenValidatedHandler(context).ConfigureAwait(false);
                };

                if (subscribeToJwtBearerMiddlewareDiagnosticsEvents)
                {
                    options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);
                }
            });

            return builder;
        }

        /// <summary>
        /// Protects the Web API with Microsoft identity platform (formerly Azure AD v2.0)
        /// This supposes that the configuration files have a section named configSectionName (typically "AzureAD")
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="configuration">Configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddProtectedWebApiCallsProtectedWebApi(
            this IServiceCollection services,
            IConfiguration configuration,
            string configSectionName = "AzureAd",
            string jwtBearerScheme = JwtBearerDefaults.AuthenticationScheme)
        {
            services.AddTokenAcquisition();
            services.AddHttpContextAccessor();
            services.Configure<ConfidentialClientApplicationOptions>(options => configuration.Bind(configSectionName, options));
            services.Configure<MicrosoftIdentityOptions>(options => configuration.Bind(configSectionName, options));

            services.Configure<JwtBearerOptions>(jwtBearerScheme, options =>
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
    }
}