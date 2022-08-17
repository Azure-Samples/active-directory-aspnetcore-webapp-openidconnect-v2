using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;

namespace WebApp_OpenIDConnect_DotNet_graph
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

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                .AddInMemoryTokenCaches();

            // uncomment the following 3 lines to get ClientSecret from KeyVault
            //string tenantId = Configuration.GetValue<string>("AzureAd:TenantId");
            //services.Configure<MicrosoftIdentityOptions>(
            //   options => { options.ClientSecret = GetSecretFromKeyVault(tenantId, "ENTER_YOUR_SECRET_NAME_HERE"); });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddRazorPages()
                  .AddMicrosoftIdentityUI();

            // Add the UI support to handle claims challenges
            services.AddServerSideBlazor()
               .AddMicrosoftIdentityConsentHandler();
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

        /// Gets the secret from key vault via an enabled Managed Identity.
        /// </summary>
        /// <remarks>https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet/blob/master/README.md</remarks>
        /// <returns></returns>
        private string GetSecretFromKeyVault(string tenantId, string secretName)
        {
            // this should point to your vault's URI, like https://<yourkeyvault>.vault.azure.net/
            string uri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
            DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions();

            // Specify the tenant ID to use the dev credentials when running the app locally
            options.VisualStudioTenantId = tenantId;
            options.SharedTokenCacheTenantId = tenantId;
            SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential(options));

            // The secret name, for example if the full url to the secret is https://<yourkeyvault>.vault.azure.net/secrets/ENTER_YOUR_SECRET_NAME_HERE
            Response<KeyVaultSecret> secret = client.GetSecretAsync(secretName).Result;

            return secret.Value.Value;
        }
    }
}