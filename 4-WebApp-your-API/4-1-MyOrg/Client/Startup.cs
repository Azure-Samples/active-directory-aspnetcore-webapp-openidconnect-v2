// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Linq;
using System.Diagnostics;

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
            services.AddDistributedMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            services.AddOptions();

            // The following lines of code adds the ability to authenticate users of this web app.
            // Refer to https://github.com/AzureAD/microsoft-identity-web/wiki/web-apps to learn more
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi(
                        Configuration.GetSection("TodoList:Scopes").Get<string[]>()
                     )
                    .AddInMemoryTokenCaches();

            // This is how we configure certificates in startup - see README-use-certificate.md for more details on how to use this section
            // Also read more at  - https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates
            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApp(microsoftIdentityOptions =>
            //    {
            //        Configuration.Bind("AzureAd", microsoftIdentityOptions);
            //        microsoftIdentityOptions.ClientCertificates = new CertificateDescription[] {
            //        CertificateDescription.FromKeyVault("[Enter URL for you Key Vault]",
            //                                            "TodoListClient-aspnetcore-webapi")};
            //    })
            //    .EnableTokenAcquisitionToCallDownstreamApi(confidentialClientApplicationOptions =>
            //    {
            //        Configuration.GetSection("TodoList:TodoListScopes").Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
            //        Configuration.Bind("AzureAd", confidentialClientApplicationOptions);
            //    })
            //      .AddInMemoryTokenCaches();


            // Add APIs
            services.AddDownstreamApi("TodoList", Configuration.GetSection("TodoList"));


            // The following flag can be used to get more descriptive errors in development environments
            // Enable diagnostic logging to help with troubleshooting.  For more details, see https://aka.ms/IdentityModel/PII.
            // You might not want to keep this following flag on for production
            IdentityModelEventSource.ShowPII = true;

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
            //app.UseCookiePolicy();

            app.UseRouting();
            app.UseAuthentication();

            app.Use(async (context, next) => {
                if (context.User != null && context.User.Identity.IsAuthenticated)
                {
                    // you can conduct any conditional processing for guest/homes user by inspecting the value of the 'acct' claim
                    // Read more about the 'acct' claim at aka.ms/optionalclaims
                    if (context.User.Claims.Any(x => x.Type == "acct"))
                    {
                        string claimvalue = context.User.Claims.FirstOrDefault(x => x.Type == "acct").Value;
                        string userType = claimvalue == "0" ? "Member" : "Guest";
                        Debug.WriteLine($"The type of the user account from this Azure AD tenant is-{userType}");
                    }
                }
                await next();
            });


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}