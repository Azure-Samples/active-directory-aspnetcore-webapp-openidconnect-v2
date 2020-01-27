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
        [Obsolete("Use AddSignIn")]
        public static IServiceCollection AddMicrosoftIdentityPlatform(
                this IServiceCollection services,
                IConfiguration configuration,
                string configSectionName = "AzureAd",
                bool subscribeToOpenIdConnectMiddlewareDiagnosticsEvents = false)
        {
            return AddSignIn(services, 
                             configuration, 
                             configSectionName, 
                             subscribeToOpenIdConnectMiddlewareDiagnosticsEvents);
        }

        [Obsolete("Use AddWebAppCallsProtectedWebApi")]
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
        /// Add authentication with Microsoft identity platform.
        /// This method expects the configuration file will have a section named "AzureAd" with the necessary settings to initialize authentication options.
        /// </summary>
        /// <param name="services">Service collection to which to add this authentication scheme</param>
        /// <param name="configuration">The Configuration object</param>
        /// <param name="subscribeToOpenIdConnectMiddlewareDiagnosticsEvents">
        /// Set to true if you want to debug, or just understand the OpenIdConnect events.
        /// </param>
        /// <returns></returns>
        //public static IServiceCollection AddSignIn(
        //    this IServiceCollection services,
        //    IConfiguration configuration,
        //    string configSectionName = "AzureAD",
        //    bool subscribeToOpenIdConnectMiddlewareDiagnosticsEvents = false)
        //{

        //    services.AddAuthentication(auth =>
        //    {
        //        auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //        auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        //        auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //    })
        //    .AddCookie()
        //    .AddOpenIdConnect(options =>
        //    {
        //        configuration.GetSection(configSectionName).Bind(options);

        //        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        //        services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
        //        {
        //        // Per the code below, this application signs in users in any Work and School
        //        // account and any Microsoft personal account.
        //        // If you want to direct Azure AD to restrict the users that can sign-in, change
        //        // the tenant value of the appsettings.json file in the following way:
        //        // - only Work and School accounts => 'organizations'
        //        // - only Microsoft personal accounts => 'consumers'
        //        // - Work and School and personal accounts => 'common'
        //        // If you want to restrict the users that can sign-in to only one tenant,
        //        // set the tenant value in the appsettings.json file to the tenant ID
        //        // or domain of that organization
        //        options.Authority = options.Authority + "/v2.0/";

        //        // If you want to restrict the users that can sign-in to several organizations
        //        // Set the tenant value in the appsettings.json file to 'organizations', and add the
        //        // issuers you want to accept to options.TokenValidationParameters.ValidIssuers collection
        //        options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;

        //        // Set the nameClaimType to be preferred_username.
        //        // This change is needed because certain token claims from the Azure AD V1 endpoint
        //        // (on which the original .NET core template is based) are different than Microsoft identity platform endpoint.
        //        // For more details see [ID Tokens](https://docs.microsoft.com/azure/active-directory/develop/id-tokens)
        //        // and [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens)
        //        options.TokenValidationParameters.NameClaimType = "preferred_username";

        //        // Avoids having users being presented the select account dialog when they are already signed-in
        //        // for instance when going through incremental consent
        //        options.Events.OnRedirectToIdentityProvider = context =>
        //            {
        //                var login = context.Properties.GetParameter<string>(OpenIdConnectParameterNames.LoginHint);
        //                if (!string.IsNullOrWhiteSpace(login))
        //                {
        //                    context.ProtocolMessage.LoginHint = login;
        //                    context.ProtocolMessage.DomainHint = context.Properties.GetParameter<string>(
        //                        OpenIdConnectParameterNames.DomainHint);

        //                // delete the login_hint and domainHint from the Properties when we are done otherwise
        //                // it will take up extra space in the cookie.
        //                context.Properties.Parameters.Remove(OpenIdConnectParameterNames.LoginHint);
        //                    context.Properties.Parameters.Remove(OpenIdConnectParameterNames.DomainHint);
        //                }

        //            // Additional claims
        //            if (context.Properties.Items.ContainsKey(OidcConstants.AdditionalClaims))
        //                {
        //                    context.ProtocolMessage.SetParameter(
        //                        OidcConstants.AdditionalClaims,
        //                        context.Properties.Items[OidcConstants.AdditionalClaims]);
        //                }

        //                return Task.FromResult(0);
        //            };

        //            if (subscribeToOpenIdConnectMiddlewareDiagnosticsEvents)
        //            {
        //                OpenIdConnectMiddlewareDiagnostics.Subscribe(options.Events);
        //            }
        //        });

        //        return services;
        //    }

        // TODO: pass an option with a section name to bind the options ? or a delegate?

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
                var handler = options.Events.OnAuthorizationCodeReceived;
                options.Events.OnAuthorizationCodeReceived = async context =>
                {
                    var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await tokenAcquisition.AddAccountToCacheFromAuthorizationCodeAsync(context, options.Scope).ConfigureAwait(false);
                    await handler(context).ConfigureAwait(false);
                };

                // Handling the sign-out: removing the account from MSAL.NET cache
                options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
                {
                    // Remove the account from MSAL.NET token cache
                    var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await tokenAcquisition.RemoveAccountAsync(context).ConfigureAwait(false);
                };
            });
            return services;
        }

        /// <summary>
        /// Handles SameSite cookie issue according to the https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1.
        /// The default list of user-agents that disallow SameSite None, was taken from https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static CookiePolicyOptions HandleSameSiteCookieCompatibility(this CookiePolicyOptions options)
        {
            return HandleSameSiteCookieCompatibility(options, DisallowsSameSiteNone);
        }

        /// <summary>
        /// Handles SameSite cookie issue according to the docs: https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
        /// The default list of user-agents that disallow SameSite None, was taken from https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
        /// </summary>
        /// <param name="options"></param>
        /// <param name="disallowsSameSiteNone">If you dont want to use the default user-agent list implementation, the method sent in this parameter will be run against the user-agent and if returned true, SameSite value will be set to Unspecified. The default user-agent list used can be found at: https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/</param>
        /// <returns></returns>
        public static CookiePolicyOptions HandleSameSiteCookieCompatibility(this CookiePolicyOptions options, Func<string, bool> disallowsSameSiteNone)
        {
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            options.OnAppendCookie = cookieContext =>
                CheckSameSite(cookieContext.Context, cookieContext.CookieOptions, disallowsSameSiteNone);
            options.OnDeleteCookie = cookieContext =>
                CheckSameSite(cookieContext.Context, cookieContext.CookieOptions, disallowsSameSiteNone);
            return options;
        }

        private static void CheckSameSite(HttpContext httpContext, CookieOptions options, Func<string, bool> disallowsSameSiteNone)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (disallowsSameSiteNone(userAgent))
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

        // Method taken from https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
        public static bool DisallowsSameSiteNone(string userAgent)
        {
            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking
            // stack.
            if (userAgent.Contains("CPU iPhone OS 12") ||
                userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
            // This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }
        
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
                
                options.TokenValidationParameters.NameClaimType = "preferred_username";

                // If you want to restrict the users that can sign-in to several organizations
                // Set the tenant value in the appsettings.json file to 'organizations', and add the
                // issuers you want to accept to options.TokenValidationParameters.ValidIssuers collection
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;

                // Avoids having users being presented the select account dialog when they are already signed-in
                // for instance when going through incremental consent
                options.Events.OnRedirectToIdentityProvider = context =>
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
                        // When a new Challenge is returned using any B2C policy different than sisu, we must change
                        // the ProtocolMessage.IssuerAddress to the desired policy otherwise the redirect would use the sisu policy
                        b2COidcHandlers.OnRedirectToIdentityProvider(context);
                    }

                    return Task.FromResult(0);
                };

                if (microsoftIdentityOptions.IsB2C)
                {
                    // Handles the error when a user cancels an action on the Azure Active Directory B2C UI.
                    // Handle the error code that Azure Active Directory B2C throws when trying to reset a password from the login page 
                    // because password reset is not supported by a "sign-up or sign-in policy".
                    options.Events.OnRemoteFailure = b2COidcHandlers.OnRemoteFailure;
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
