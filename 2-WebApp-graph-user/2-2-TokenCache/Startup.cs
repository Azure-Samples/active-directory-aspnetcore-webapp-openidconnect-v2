using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using System;

namespace _2_1_Call_MSGraph
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
            string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');


            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Execute the following commands to initialize the sql server database with tables required to cache tokens.
            // NOTE : This is a one time use method. We advise using it in development environments to create the tables required to enable token caching.
            // For production deployments, preferably, generate the schema from the tables generated in dev environments and use it to create the necessary tables in production.
            /*
             *  1. For instance in Visual Studio, open the SQL Server Object explorer, then (localdb)\MSSQLLocalDB, then databases
             *  2. Right click on Databases and select "Add New database", and then choose the name of the database: 'MsalTokenCacheDatabase'
             *  3. In the console application run the 2 following commands:
                     dotnet tool install --global dotnet-sql-cache
                     dotnet sql-cache create "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MsalTokenCacheDatabase;Integrated Security=True;" dbo TokenCache
             */

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                        .AddDistributedTokenCaches();

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("TokenCacheDbConnStr");
                options.SchemaName = "dbo";
                options.TableName = "TokenCache";

                // You don't want the SQL token cache to be purged before the access token has expired. Usually
                // access tokens expire after 1 hour (but this can be changed by token lifetime policies), whereas

                // the default sliding expiration for the distributed SQL database is 20 mins. 
                // Use a value which is above 60 mins (or the lifetime of a token in case of longer lived tokens)
                options.DefaultSlidingExpiration = TimeSpan.FromMinutes(90);
            });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddRazorPages()
                .AddMicrosoftIdentityUI();
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
