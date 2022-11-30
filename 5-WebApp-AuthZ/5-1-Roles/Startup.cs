using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System.IdentityModel.Tokens.Jwt;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;
using Microsoft.Identity.Web.UI;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using System;

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

            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;


            // Sign-in users with the Microsoft identity platform
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi(Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' '))
                    .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                    .AddInMemoryTokenCaches();

            // The following lines code instruct the asp.net core middleware to use the data in the "roles" claim in the Authorize attribute and User.IsInrole()
            // See https://docs.microsoft.com/aspnet/core/security/authorization/roles?view=aspnetcore-2.2 for more info.
            services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // The claim in the Jwt token where App roles are available.
                options.TokenValidationParameters.RoleClaimType = "roles";
            });

            // Adding authorization policies that enforce authorization using Azure AD roles.
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.AssignmentToUserReaderRoleRequired, policy => policy.RequireRole(AppRole.UserReaders));
                options.AddPolicy(AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired, policy => policy.RequireRole(AppRole.DirectoryViewers));
            });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            services.AddHttpContextAccessor();
            services.AddRazorPages();

            services.AddServerSideBlazor()
               .AddMicrosoftIdentityConsentHandler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; ;
                        context.Response.ContentType = "text/html";

                        await context.Response.WriteAsync("<html lang=\"en\"><head>\r\n");

                        await context.Response.WriteAsync("</head>\r\n");
                        await context.Response.WriteAsync("<meta name=\"viewport\" content=\"width = device - width, initial - scale = 1\"><style>.collapsible{color: #777;cursor: pointer;padding: 18px;width: 100%;border: none;text-align: left;outline: none;font-size: 15px;}.active, .collapsible:hover {background-color: #555;}.collapsible:after {content: '\\002B';color: #777;font-weight: bold;float: right;margin-left: 5px;}.active:after {content: \"\\2212\";}.content {padding: 0 18px;max-height: 0;overflow: hidden;transition: max-height 0.2s ease-out;background-color: #f1f1f1;}</style>");
                        await context.Response.WriteAsync("<body>\r\n");
                        await context.Response.WriteAsync("<a href=\"/\">Back to Home</a><br>\r\n");
                        await context.Response.WriteAsync("<h2>Exception Overview</h2><br>\r\n");

                        var exceptionHandlerPathFeature =
                            context.Features.Get<IExceptionHandlerPathFeature>();
                        if (exceptionHandlerPathFeature?.Error is Exception)
                        {
                            if (exceptionHandlerPathFeature.Error.InnerException != null)
                            {
                                if (exceptionHandlerPathFeature.Error.InnerException.Message.Contains(Constants.UserConsentDeclinedError))
                                {
                                    await context.Response.WriteAsync($"<h3>{Constants.UserConsentDeclinedErrorMessage}</h3><br>\r\n");
                                }
                                else
                                {
                                    await context.Response.WriteAsync($"<h3>{exceptionHandlerPathFeature?.Error.InnerException?.Message}</h3><br>\r\n");

                                }
                            }
                            else
                            {
                                await context.Response.WriteAsync($"<h3>{exceptionHandlerPathFeature?.Error.Message}</h3><br>\r\n");
                            }

                            await context.Response.WriteAsync("<button class=\"collapsible\">Click for Exception Full Stack</button>\r\n");
                            await context.Response.WriteAsync("<div class=\"content\"><p>");
                            await context.Response.WriteAsync(exceptionHandlerPathFeature?.Error.StackTrace);
                            await context.Response.WriteAsync("</p></div>");
                        }

                        await context.Response.WriteAsync("<script>var coll = document.getElementsByClassName(\"collapsible\");var i;for (i = 0; i < coll.length; i++) {coll[i].addEventListener(\"click\", function() {this.classList.toggle(\"active\");var content = this.nextElementSibling;if (content.style.maxHeight){content.style.maxHeight = null;} else {content.style.maxHeight = content.scrollHeight + \"px\";}});}</script>");
                        await context.Response.WriteAsync("</body></html>\r\n");
                    });
                });
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