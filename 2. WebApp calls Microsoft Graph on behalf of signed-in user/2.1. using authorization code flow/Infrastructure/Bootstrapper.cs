using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Client;
using WebApp_OpenIDConnect_DotNet.Interfaces;
using WebApp_OpenIDConnect_DotNet.Services;

namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public static class Bootstrapper
    {
        public static void InitializeDefault(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CookiePolicyOptions>(options =>
                                                    {
                                                        options.CheckConsentNeeded = context => true;
                                                        options.MinimumSameSitePolicy = SameSiteMode.None;
                                                    });
            services.Configure<WebOptions>(configuration);
            //            services.Configure<AzureADOptions>(configuration.GetSection("AzureAd"));
            //https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<IGraphApiOperations, GraphApiOperationService>();
        }

        public static void InitializeAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Token acquisition service and its cache implementation
            services.AadAzureAdV2Authentication(configuration)
                    .WithMsal(new string[] { Constants.ScopeUserRead })
                    .AddDistributedMemoryCache()
                    .AddInMemoryTokenCache()
                    /* you could use a cookie based token cache by reaplacing the last
                     * trew lines by : .AddCookie().AddCookieBasedTokenCache()  */
                    ;

            services.AddMvc(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }
    }
}