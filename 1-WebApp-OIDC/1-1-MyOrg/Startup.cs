using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;

namespace WebApp_OpenIDConnect_DotNet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            // Sign-in users with the Microsoft identity platform
            services.AddSignIn(options => 
            {
                Configuration.Bind("AzureAd", options);
                options.MetadataAddress = "https://sts.cxpaadtenant.com/adfs/.well-known/openid-configuration";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                options.TokenValidationParameters.ValidIssuer = "https://sts.cxpaadtenant.com/adfs";
                options.TokenValidationParameters.IssuerValidator = ValidateAFDSIssuer;
                options.TokenValidationParameters.NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            }, options => 
            {
                Configuration.Bind("AzureAd", options);
            });

            //services.AddWebAppCallsProtectedWebApi(Configuration, new string[] { "https://sts.cxpaadtenant.com/adfs/service/trust/openid" })
            //    .AddInMemoryTokenCaches();

            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
            //        Configuration.Bind("AzureAd", options);
            //        options.MetadataAddress = "https://sts.cxpaadtenant.com/adfs/.well-known/openid-configuration";
            //        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private string ValidateAFDSIssuer(string actualIssuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (string.IsNullOrEmpty(actualIssuer))
                throw new ArgumentNullException(nameof(actualIssuer));

            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            if (validationParameters == null)
                throw new ArgumentNullException(nameof(validationParameters));

            if (validationParameters.ValidIssuer == actualIssuer)
                return actualIssuer;

            // If a valid issuer is not found, throw
            // brentsch - todo, create a list of all the possible valid issuers in TokenValidationParameters
            throw new SecurityTokenInvalidIssuerException($"Issuer: '{actualIssuer}', does not match any of the valid issuers provided for this application.");
        }
    }
}
