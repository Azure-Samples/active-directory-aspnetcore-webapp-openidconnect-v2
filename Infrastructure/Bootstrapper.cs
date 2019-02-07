using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp_OpenIDConnect_DotNet.Interfaces;
using WebApp_OpenIDConnect_DotNet.Services;

namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public static class Bootstrapper
    {
        public static void InitializeDefault(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CookiePolicyOptions>(options =>
                                                    {
                                                        options.CheckConsentNeeded    = context => true;
                                                        options.MinimumSameSitePolicy = SameSiteMode.None;
                                                    });
            services.Configure<WebOptions>(configuration);
            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));
            //https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<IGraphApiOperations, GraphApiOperationService>();
        }

        public static void InitializeAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => configuration.Bind("AzureAd", options));

            // Token acquisition service and its cache implementation
            services.WithOpenIdConnect()
                    .AddTokenAcquisition()
                    .AddDistributedMemoryCache()
                    .AddInMemoryTokenCache()
                    /* you could use a cookie based token cache by reaplacing the last
                     * trew lines by : .AddCookie().AddCookieBasedTokenCache()  */
                    ;

            services.AddMvc(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public static IServiceCollection WithOpenIdConnect(this IServiceCollection services)
        {
            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.Authority = options.Authority + "/v2.0/";

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
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.ValidateAadIssuer;

                // If you want to restrict the users that can sign-in to several organizations
                // Set the tenant value in the appsettings.json file to 'organizations', and add the
                // issuers you want to accept to options.TokenValidationParameters.ValidIssuers collection

                // Response type
                options.ResponseType = Constants.OpenIdResponseType;
                options.Scope.Add(Constants.ScopeOfflineAccess);
                options.Scope.Add(Constants.ScopeUserRead);
                //                options.Prompt = "consent";

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
                    var user = context.HttpContext.User;

                    // Avoid displaying the select account dialog
                    context.ProtocolMessage.LoginHint = user.GetLoginHint();
                    context.ProtocolMessage.DomainHint = user.GetDomainHint();

                    // Remove the account from MSAL.NET token cache
                    var _tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await _tokenAcquisition.RemoveAccount(context);
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
                    if (context.Properties.Items.ContainsKey(Constants.AdditionalClaims))
                    {
                        context.ProtocolMessage.SetParameter(Constants.AdditionalClaims, 
                                                             context.Properties.Items[Constants.AdditionalClaims]);
                    }

                    return Task.FromResult(0);
                };

                // If you want to debug, or just understand the OpenIdConnect events, just
                // uncomment the following line of code
                // OpenIdConnectMiddlewareDiagnostics.Subscribe(options.Events);
            });
            return services;
        }
    }
}