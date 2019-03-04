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
        public static void AddGraphService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebOptions>(configuration);
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<IGraphApiOperations, GraphApiOperationService>();
        }
    }
}