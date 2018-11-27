using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => Configuration.Bind("AzureAd", options))
                ;

            // Token acquisition service and its cache implementation
            services.AddTokenAcquisition()
                    .AddDistributedMemoryCache()
                    .AddSession()
                    .AddSessionBasedTokenCache()
                    /* you could use a cookie based token cache by reaplacing the last
                     * trew lines by : .AddCookie().AddCookieBasedTokenCache()  */
                    ;

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
                options.ResponseType = "id_token code";
                options.Scope.Add("offline_access");
                options.Scope.Add("User.Read");
//                options.Prompt = "consent";
                
                // Handling the auth code
                var handler = options.Events.OnAuthorizationCodeReceived;
                options.Events.OnAuthorizationCodeReceived = async context =>
                {
                    var _tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, options.Scope);
                    await handler(context);
                };
            });

            services.AddMvc(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.1#configure-session-state
            // to use a session token cache
            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
