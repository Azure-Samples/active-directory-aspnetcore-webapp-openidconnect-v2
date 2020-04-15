// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.DAL;
using WebApp_OpenIDConnect_DotNet.Services;
using WebApp_OpenIDConnect_DotNet.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web.UI;

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

            services.AddOptions();

            //If you want to run this sample using in memory db, uncomment the line below (options.UseInMemoryDatabase) and comment the one that uses options.UseSqlServer.
            //services.AddDbContext<SampleDbContext>(options => options.UseInMemoryDatabase(databaseName: "MultiTenantOnboarding"));
            services.AddDbContext<SampleDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SampleDbConnStr")));

            services.AddScoped<ITodoItemService, TodoItemService>();

            // Add Microsoft Graph support
            services.AddScoped<IMSGraphService, MSGraphService>();

            // Sign-in users with the Microsoft identity platform
            services.AddSignIn(options =>
                {
                    Configuration.Bind("AzureAd", options);
                    options.Events.OnTokenValidated = async context =>
                    {
                        string tenantId = context.SecurityToken.Claims.FirstOrDefault(x => x.Type == "tid" || x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

                        if (string.IsNullOrWhiteSpace(tenantId))
                            throw new UnauthorizedAccessException("Unable to get tenantId from token.");

                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<SampleDbContext>();

                        var authorizedTenant = await dbContext.AuthorizedTenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);

                        if (authorizedTenant == null)
                            throw new UnauthorizedTenantException("This tenant is not authorized");
                    };
                    options.Events.OnAuthenticationFailed = (context) =>
                    {
                        if (context.Exception != null && context.Exception is UnauthorizedTenantException)
                        {
                            context.Response.Redirect("/Home/UnauthorizedTenant");
                            context.HandleResponse(); // Suppress the exception
                        }

                        return Task.FromResult(0);
                    };
                }, options =>
                {
                    Configuration.Bind("AzureAD", options);
                });

            services.AddWebAppCallsProtectedWebApi(Configuration, new string[] { GraphScope.UserReadAll })
                .AddInMemoryTokenCaches();

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
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<SampleDbContext>();
                // Uncomment, run the solution, and comment again to reset the DB
                //context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

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
    }
}