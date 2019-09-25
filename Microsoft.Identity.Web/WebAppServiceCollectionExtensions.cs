// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extensions for IServiceCollection for startup initialization.
    /// </summary>
    public static class WebAppServiceCollectionExtensions
    {
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
        public static IServiceCollection AddMicrosoftIdentityPlatformAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            string configSectionName = "AzureAd",
            bool subscribeToOpenIdConnectMiddlewareDiagnosticsEvents = false)
        {

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => configuration.Bind(configSectionName, options));
            services.Configure<AzureADOptions>(options => configuration.Bind(configSectionName, options));

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                // Per the code below, this application signs in users in any Work and School
                // accounts and any Microsoft Personal Accounts.
                // If you want to direct Azure AD to restrict the users that can sign-in, change
                // the tenant value of the appsettings.json file in the following way:
                // - only Work and School accounts => 'organizations'
                // - only Microsoft Personal accounts => 'consumers'
                // - Work and School and Personal accounts => 'common'
                // If you want to restrict the users that can sign-in to only one tenant
                // set the tenant value in the appsettings.json file to the tenant ID
                // or domain of this organization
                options.Authority = options.Authority + "/v2.0/";

                // If you want to restrict the users that can sign-in to several organizations
                // Set the tenant value in the appsettings.json file to 'organizations', and add the
                // issuers you want to accept to options.TokenValidationParameters.ValidIssuers collection
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;

                // Set the nameClaimType to be preferred_username.
                // This change is needed because certain token claims from Azure AD V1 endpoint
                // (on which the original .NET core template is based) are different than Microsoft identity platform endpoint.
                // For more details see [ID Tokens](https://docs.microsoft.com/azure/active-directory/develop/id-tokens)
                // and [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens)
                options.TokenValidationParameters.NameClaimType = "preferred_username";
                
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

                    return Task.FromResult(0);
                };

                if (subscribeToOpenIdConnectMiddlewareDiagnosticsEvents)
                {
                    OpenIdConnectMiddlewareDiagnostics.Subscribe(options.Events);
                }
            });
            return services;
        }

        // TODO: pass an option with a section name to bind the options ? or a delegate?

        /// <summary>
        /// Add MSAL support to the Web App or Web API
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="initialScopes">Initial scopes to request at sign-in</param>
        /// <returns></returns>
        public static IServiceCollection AddMsal(this IServiceCollection services, IConfiguration configuration, IEnumerable<string> initialScopes, string configSectionName = "AzureAd")
        {
            // Ensure that configuration options for MSAL.NET, HttpContext accessor and the Token acquisition service
            // (encapsulating MSAL.NET) are available through dependency injection
            services.Configure<ConfidentialClientApplicationOptions>(options => configuration.Bind(configSectionName, options));
            services.AddHttpContextAccessor();
            services.AddTokenAcquisition();

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                // Response type
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                // This scope is needed to get a refresh token when users sign-in with their Microsoft personal accounts
                // (it's required by MSAL.NET and automatically provided when users sign-in with work or school accounts)
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
    }
}
