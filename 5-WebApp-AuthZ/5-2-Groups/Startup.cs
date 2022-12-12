using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Services;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet
{
    public class Startup
    {
        public CacheSettings cacheSettings;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            cacheSettings = new CacheSettings
            {
                SlidingExpirationInSeconds = Configuration.GetValue<string>("CacheSettings:SlidingExpirationInSeconds"),
                AbsoluteExpirationInSeconds = Configuration.GetValue<string>("CacheSettings:AbsoluteExpirationInSeconds")
            };
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var initialScopes = new string[] { Constants.ScopeUserRead, Constants.ScopeGroupMemberRead };

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite
                options.HandleSameSiteCookieCompatibility();
            });

            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                    {
                        // Ensure default token validation is carried out
                        Configuration.Bind("AzureAd", options);

                        // The following lines code instruct the asp.net core middleware to use the data in the "roles" claim in the [Authorize] attribute, policy.RequireRole() and User.IsInRole()
                        // See https://docs.microsoft.com/aspnet/core/security/authorization/roles for more info.
                        options.TokenValidationParameters.RoleClaimType = "groups";

                        /// <summary>
                        /// Below you can do extended token validation and check for additional claims, such as:
                        ///
                        /// - check if the caller's tenant is in the allowed tenants list via the 'tid' claim (for multi-tenant applications)
                        /// - check if the caller's account is homed or guest via the 'acct' optional claim
                        /// - check if the caller belongs to right roles or groups via the 'roles' or 'groups' claim, respectively
                        ///
                        /// Bear in mind that you can do any of the above checks within the individual routes and/or controllers as well.
                        /// For more information, visit: https://docs.microsoft.com/azure/active-directory/develop/access-tokens#validate-the-user-has-permission-to-access-this-data
                        /// </summary>

                        options.Events.OnTokenValidated = async context =>
                        {
                            if (context != null)
                            {
                                List<string> requiredGroupsIds = Configuration.GetSection("AzureAd:Groups")
                                    .AsEnumerable().Select(x => x.Value).Where(x => x != null).ToList();

                                // Calls method to process groups overage claim (before policy checks kick-in)
                                await GraphHelper.ProcessAnyGroupsOverage(context, requiredGroupsIds, cacheSettings);
                            }

                            await Task.CompletedTask;
                        };
                    })
                .EnableTokenAcquisitionToCallDownstreamApi(options => Configuration.Bind("AzureAd", options), initialScopes)
                .AddMicrosoftGraph(Configuration.GetSection("GraphAPI"))
                .AddInMemoryTokenCaches();

            // Adding authorization policies that enforce authorization using Azure AD security groups.
            services.AddAuthorization(options =>
            {
                // this policy stipulates that users in both GroupMember and GroupAdmin can access resources
                options.AddPolicy(AuthorizationPolicies.AssignmentToGroupMemberGroupRequired, policy => policy.RequireRole(Configuration["Groups:GroupMember"], Configuration["Groups:GroupAdmin"]));

                // this policy stipulates that users in GroupAdmin can access resources
                options.AddPolicy(AuthorizationPolicies.AssignmentToGroupAdminGroupRequired, policy => policy.RequireRole(Configuration["Groups:GroupAdmin"]));
            });

            services.AddControllers();
            services.AddHttpContextAccessor();

            // The following flag can be used to get more descriptive errors in development environments
            // Enable diagnostic logging to help with troubleshooting. For more details, see https://aka.ms/IdentityModel/PII.
            // You might not want to keep this following flag on for production
            IdentityModelEventSource.ShowPII = true;

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }).AddMicrosoftIdentityUI();

            services.AddRazorPages();

            // Add the UI support to handle claims challenges
            services.AddServerSideBlazor()
               .AddMicrosoftIdentityConsentHandler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // Personal Identifiable Information is not written to the logs by default, to be compliant with GDPR.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.

                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = false;

                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
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