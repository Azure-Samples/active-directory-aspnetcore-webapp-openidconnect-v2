using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Services;

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
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddOptions();

            // Token acquisition service based on MSAL.NET
            // and chosen token cache implementation
            services.AddMicrosoftIdentityPlatformAuthentication(Configuration)
               .AddMsal(Configuration, new string[] { Constants.ScopeUserRead })
               .AddInMemoryTokenCaches();

            services.Configure<ConfidentialClientApplicationOptionsWithClientCertificate>(options =>
              {
                  options.ClientCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                  options.ClientSecret = null;
              }
            );

            /*
               // or use a distributed Token Cache by adding 
                           .AddDistributedTokenCaches();

               // and then choose your implementation. 
               // See https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-2.2#distributed-memory-cache

               // For instance the distributed in memory cache (not cleared when you stop the app)
                services.AddDistributedMemoryCache()

               // Or a Redis cache
               services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = "localhost";
                        options.InstanceName = "SampleInstance";
                    });

               // Or even a SQL Server token cache
               services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString = 
                        _config["DistCache_ConnectionString"];
                    options.SchemaName = "dbo";
                    options.TableName = "TestCache";
                });
            */
            // Add Graph
            services.AddGraphService(Configuration);

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}