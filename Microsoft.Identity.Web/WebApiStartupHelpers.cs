using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web.Client;
using Microsoft.Identity.Web.Resource;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    public static class WebApiStartupHelpers
    {
        /// <summary>
        /// Protects the Web API with Microsoft Identity Platform v2.0 (AAD v2.0)
        /// This supposes that the configuration files have a section named "AzureAD"
        /// </summary>
        /// <param name="services">Service collection to which to add authentication</param>
        /// <param name="configuration">Configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddProtectWebApiWithMicrosoftIdentityPlatformV2(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                    .AddAzureADBearer(options => configuration.Bind("AzureAd", options));

            services.AddSession();

            // Added
            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // Reinitialize the options as this has changed to JwtBearerOptions to pick configuration values for attributes unique to JwtBearerOptions 
                configuration.Bind("AzureAd", options);

                // This is an Azure AD v2.0 Web API
                options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
                options.TokenValidationParameters.ValidAudiences = new string[] { options.Audience, $"api://{options.Audience}" };

                // Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                // we inject our own multitenant validation logic (which even accepts both V1 and V2 tokens)
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).ValidateAadIssuer;

                // When an access token for our own Web API is validated, we add it to MSAL.NET's cache so that it can
                // be used from the controllers.
                options.Events = new JwtBearerEvents();

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
        public static IServiceCollection AddProtectedApiCallsWebApis(this IServiceCollection services, IConfiguration configuration, IEnumerable<string> scopes=null)
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
                        (context.Principal.Identity as ClaimsIdentity).AddClaim(new Claim("jwt", (context.SecurityToken as JwtSecurityToken).RawData));
                    }
                    // Adds the token to the cache, and also handles the incremental consent and claim challenges
                    await Task.FromResult(0);
                };
            });

            return services;
        }
    }
}