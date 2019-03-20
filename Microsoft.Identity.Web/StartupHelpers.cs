using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Client;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    public static class StartupHelpers
    {
        /// <summary>
        /// Add authentication with Microsoft identity platform v2.0 (AAD v2.0).
        /// This supposes that the configuration files have a section named "AzureAD"
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="configuration">Configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureAdV2Authentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => configuration.Bind("AzureAd", options));

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
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.ForAadInstance(options.Authority).ValidateAadIssuer;

                // Set the nameClaimType to be preferred_username.
                // This change is needed because certain token claims from Azure AD V1 endpoint 
                // (on which the original .NET core template is based) are different than Azure AD V2 endpoint. 
                // For more details see [ID Tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens) 
                // and [Access Tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens)
                options.TokenValidationParameters.NameClaimType = "preferred_username";

                // Handling the sign-out: removing the account from MSAL.NET cache
                options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
                {
                    var user = context.HttpContext.User;

                    // Avoid displaying the select account dialog
                    context.ProtocolMessage.LoginHint = user.GetLoginHint();
                    context.ProtocolMessage.DomainHint = user.GetDomainHint();
                    await Task.FromResult(0);
                };

                // Avoids having users being presented the select account dialog when they are already signed-in
                // for instance when going through incremental consent 
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    var login = context.Properties.GetParameter<string>(OpenIdConnectParameterNames.LoginHint);
                    if (!string.IsNullOrWhiteSpace(login))
                    {
                        context.ProtocolMessage.LoginHint = login;
                        context.ProtocolMessage.DomainHint = context.Properties.GetParameter<string>(OpenIdConnectParameterNames.DomainHint);

                        // delete the loginhint and domainHint from the Properties when we are done otherwise 
                        // it will take up extra space in the cookie.
                        context.Properties.Parameters.Remove(OpenIdConnectParameterNames.LoginHint);
                        context.Properties.Parameters.Remove(OpenIdConnectParameterNames.DomainHint);
                    }

                    // Additional claims
                    if (context.Properties.Items.ContainsKey(OidcConstants.AdditionalClaims))
                    {
                        context.ProtocolMessage.SetParameter(OidcConstants.AdditionalClaims,
                                                             context.Properties.Items[OidcConstants.AdditionalClaims]);
                    }

                    return Task.FromResult(0);
                };

                // If you want to debug, or just understand the OpenIdConnect events, just
                // uncomment the following line of code
                // OpenIdConnectMiddlewareDiagnostics.Subscribe(options.Events);
            });
            return services;
        }

        /// <summary>
        /// Add MSAL support to the Web App or Web API
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="initialScopes">Initial scopes to request at sign-in</param>
        /// <returns></returns>
        public static IServiceCollection AddMsal(this IServiceCollection services, IEnumerable<string> initialScopes)
        {
            services.AddTokenAcquisition();

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                // Response type
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
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
                    var _tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, options.Scope);
                    await handler(context);
                };

                // Handling the sign-out: removing the account from MSAL.NET cache
                options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
                {
                    // Remove the account from MSAL.NET token cache
                    var _tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await _tokenAcquisition.RemoveAccount(context);
                };
            });
            return services;
        }
    }
}