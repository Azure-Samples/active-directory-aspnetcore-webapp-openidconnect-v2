// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extensions for IServiceCollection for startup initialization.
    /// </summary>
    public static class WebAppServiceCollectionExtensions
    {
        #region
        [Obsolete("This method has been deprecated, please use the AddSignIn() method instead.")]
        public static IServiceCollection AddMicrosoftIdentityPlatform(
                this IServiceCollection services,
                IConfiguration configuration,
                string configSectionName = "AzureAd",
                bool subscribeToOpenIdConnectMiddlewareDiagnosticsEvents = false)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddSignIn(configSectionName,
                    configuration,
                    options => configuration.Bind(configSectionName, options),
                    subscribeToOpenIdConnectMiddlewareDiagnosticsEvents);

            return services;
        }

        [Obsolete("This method has been deprecated, please use the AddWebAppCallsProtectedWebApi() method instead.")]
        public static IServiceCollection AddMsal(this IServiceCollection services,
                                                               IConfiguration configuration,
                                                               IEnumerable<string> initialScopes,
                                                               string configSectionName = "AzureAd")
        {
            return AddWebAppCallsProtectedWebApi(services,
                                                 configuration,
                                                 initialScopes,
                                                 configSectionName);
        }
        #endregion

        /// <summary>
        /// Add MSAL support to the Web App or Web API
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="initialScopes">Initial scopes to request at sign-in</param>
        /// <returns></returns>
        public static IServiceCollection AddWebAppCallsProtectedWebApi(
            this IServiceCollection services,
            IConfiguration configuration,
            IEnumerable<string> initialScopes,
            string configSectionName = "AzureAd",
            string openIdConnectScheme = OpenIdConnectDefaults.AuthenticationScheme)
        {
            // Ensure that configuration options for MSAL.NET, HttpContext accessor and the Token acquisition service
            // (encapsulating MSAL.NET) are available through dependency injection
            services.Configure<ConfidentialClientApplicationOptions>(options => configuration.Bind(configSectionName, options));
            services.Configure<MicrosoftIdentityOptions>(options => configuration.Bind(configSectionName, options));
            services.AddHttpContextAccessor();
            services.AddTokenAcquisition();

            services.Configure<OpenIdConnectOptions>(openIdConnectScheme, options =>
            {
                // Response type
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                // This scope is needed to get a refresh token when users sign-in with their Microsoft personal accounts
                // It's required by MSAL.NET and automatically provided when users sign-in with work or school accounts
                options.Scope.Add(OidcConstants.ScopeOfflineAccess);
                if (initialScopes != null)
                {
                    foreach (string scope in initialScopes)
                    {
                        if (!options.Scope.Contains(scope))
                        {
                            options.Scope.Add(scope);
                        }
                    }
                }

                // Handling the auth redemption by MSAL.NET so that a token is available in the token cache
                // where it will be usable from Controllers later (through the TokenAcquisition service)
                var codeReceivedHandler = options.Events.OnAuthorizationCodeReceived;
                options.Events.OnAuthorizationCodeReceived = async context =>
                {
                    var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await tokenAcquisition.AddAccountToCacheFromAuthorizationCodeAsync(context, options.Scope).ConfigureAwait(false);
                    await codeReceivedHandler(context).ConfigureAwait(false);
                };

                // Handling the sign-out: removing the account from MSAL.NET cache
                var signOutHandler = options.Events.OnRedirectToIdentityProviderForSignOut;
                options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
                {
                    // Remove the account from MSAL.NET token cache
                    var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await tokenAcquisition.RemoveAccountAsync(context).ConfigureAwait(false);
                    await signOutHandler(context).ConfigureAwait(false);
                };
            });
            return services;
        }

        /// <summary>
        /// Add authentication with Microsoft identity platform.
        /// This method expects the configuration file will have a section, named "AzureAd" as default, with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="builder">AuthenticationBuilder to which to add this configuration</param>
        /// <param name="configuration">The IConfiguration object</param>
        /// <param name="configureOptions">An action to configure OpenIdConnectOptions</param>
        /// <param name="subscribeToOpenIdConnectMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the OpenIdConnect events.
        /// </param>
        /// <returns></returns>
        public static AuthenticationBuilder AddSignIn(
            this AuthenticationBuilder builder,
            IConfiguration configuration,
            Action<OpenIdConnectOptions> configureOptions,
            bool subscribeToOpenIdConnectMiddlewareDiagnosticsEvents = false) =>
                builder.AddSignIn(
                    "AzureAd",
                    configuration,
                    OpenIdConnectDefaults.AuthenticationScheme,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    configureOptions,
                    subscribeToOpenIdConnectMiddlewareDiagnosticsEvents);

        /// <summary>
        /// Add authentication with Microsoft identity platform.
        /// This method expects the configuration file will have a section, named "AzureAd" as default, with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="builder">AuthenticationBuilder to which to add this configuration</param>
        /// <param name="configSectionName">The configuration section with the necessary settings to initialize authentication options</param>
        /// <param name="configuration">The IConfiguration object</param>
        /// <param name="configureOptions">An action to configure OpenIdConnectOptions</param>
        /// <param name="subscribeToOpenIdConnectMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the OpenIdConnect events.
        /// </param>
        /// <returns></returns>
        public static AuthenticationBuilder AddSignIn(
            this AuthenticationBuilder builder,
            string configSectionName,
            IConfiguration configuration,
            Action<OpenIdConnectOptions> configureOptions,
            bool subscribeToOpenIdConnectMiddlewareDiagnosticsEvents = false) =>
                builder.AddSignIn(
                    configSectionName,
                    configuration,
                    OpenIdConnectDefaults.AuthenticationScheme,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    configureOptions,
                    subscribeToOpenIdConnectMiddlewareDiagnosticsEvents);

        /// <summary>
        /// Add authentication with Microsoft identity platform.
        /// This method expects the configuration file will have a section, named "AzureAd" as default, with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="builder">AuthenticationBuilder to which to add this configuration</param>
        /// <param name="configSectionName">The configuration section with the necessary settings to initialize authentication options</param>
        /// <param name="configuration">The IConfiguration object</param>
        /// <param name="configureOptions">An action to configure OpenIdConnectOptions</param>
        /// <param name="openIdConnectScheme">The OpenIdConnect scheme name to be used. By default it uses "OpenIdConnect"</param>
        /// <param name="cookieScheme">The Cookies scheme name to be used. By default it uses "Cookies"</param>
        /// <param name="subscribeToOpenIdConnectMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the OpenIdConnect events.
        /// </param>
        /// <returns></returns>
        public static AuthenticationBuilder AddSignIn(
            this AuthenticationBuilder builder,
            string configSectionName,
            IConfiguration configuration,
            string openIdConnectScheme,
            string cookieScheme,
            Action<OpenIdConnectOptions> configureOptions,
            bool subscribeToOpenIdConnectMiddlewareDiagnosticsEvents = false)
        {
            builder.Services.Configure(openIdConnectScheme, configureOptions);
            builder.Services.Configure<MicrosoftIdentityOptions>(options => configuration.Bind(configSectionName, options));

            var microsoftIdentityOptions = configuration.GetSection(configSectionName).Get<MicrosoftIdentityOptions>();
            var b2COidcHandlers = new AzureADB2COpenIDConnectEventHandlers(openIdConnectScheme, microsoftIdentityOptions);

            builder.AddCookie(cookieScheme);
            builder.AddOpenIdConnect(openIdConnectScheme, options =>
            {
                options.SignInScheme = cookieScheme;

                if (string.IsNullOrWhiteSpace(options.Authority))
                    options.Authority = AuthorityHelpers.BuildAuthority(microsoftIdentityOptions);

                if (!AuthorityHelpers.IsV2Authority(options.Authority))
                    options.Authority += "/v2.0";

                // B2C doesn't have preferred_username claims
                if (microsoftIdentityOptions.IsB2C)
                    options.TokenValidationParameters.NameClaimType = "name";
                else
                    options.TokenValidationParameters.NameClaimType = "preferred_username";

                // If the developer registered an IssuerValidator, do not overwrite it
                if (options.TokenValidationParameters.IssuerValidator == null)
                {
                    // If you want to restrict the users that can sign-in to several organizations
                    // Set the tenant value in the appsettings.json file to 'organizations', and add the
                    // issuers you want to accept to options.TokenValidationParameters.ValidIssuers collection
                    options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;
                }

                // Avoids having users being presented the select account dialog when they are already signed-in
                // for instance when going through incremental consent
                var redirectToIdpHandler = options.Events.OnRedirectToIdentityProvider;
                options.Events.OnRedirectToIdentityProvider = async context =>
                {
                    var login = context.Properties.GetParameter<string>(OpenIdConnectParameterNames.LoginHint);
                    if (!string.IsNullOrWhiteSpace(login))
                    {
                        context.ProtocolMessage.LoginHint = login;
                        context.ProtocolMessage.DomainHint = context.Properties.GetParameter<string>(
                            OpenIdConnectParameterNames.DomainHint);

                        // delete the login_hint and domainHint from the Properties when we are done otherwise
                        // it will take up extra space in the cookie.
                        context.Properties.Parameters.Remove(OpenIdConnectParameterNames.LoginHint);
                        context.Properties.Parameters.Remove(OpenIdConnectParameterNames.DomainHint);
                    }

                    // Additional claims
                    if (context.Properties.Items.ContainsKey(OidcConstants.AdditionalClaims))
                    {
                        context.ProtocolMessage.SetParameter(
                            OidcConstants.AdditionalClaims,
                            context.Properties.Items[OidcConstants.AdditionalClaims]);
                    }

                    if (microsoftIdentityOptions.IsB2C)
                    {
                        // When a new Challenge is returned using any B2C user flow different than susi, we must change
                        // the ProtocolMessage.IssuerAddress to the desired user flow otherwise the redirect would use the susi user flow
                        await b2COidcHandlers.OnRedirectToIdentityProvider(context);
                    }

                    await redirectToIdpHandler(context).ConfigureAwait(false);
                };

                if (microsoftIdentityOptions.IsB2C)
                {
                    var remoteFailureHandler = options.Events.OnRemoteFailure;
                    options.Events.OnRemoteFailure = async context =>
                    {
                        // Handles the error when a user cancels an action on the Azure Active Directory B2C UI.
                        // Handle the error code that Azure Active Directory B2C throws when trying to reset a password from the login page 
                        // because password reset is not supported by a "sign-up or sign-in user flow".
                        await b2COidcHandlers.OnRemoteFailure(context);

                        await remoteFailureHandler(context).ConfigureAwait(false);
                    };
                }

                if (subscribeToOpenIdConnectMiddlewareDiagnosticsEvents)
                {
                    OpenIdConnectMiddlewareDiagnostics.Subscribe(options.Events);
                }
            });

            return builder;
        }
    }
}
