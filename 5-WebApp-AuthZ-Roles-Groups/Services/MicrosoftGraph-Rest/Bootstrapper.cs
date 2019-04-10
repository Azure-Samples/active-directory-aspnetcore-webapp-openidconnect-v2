using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph;

namespace WebApp_OpenIDConnect_DotNet.Services.GraphOperations
{
    public static class Bootstrapper
    {
        public static void AddGraphService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebOptions>(configuration);
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<IGraphApiOperations, GraphApiOperationService>();
        }

        /// <summary>Adds support for IMSGraphService, which provides interaction with Microsoft Graph using Graph SDK.</summary>
        /// <param name="services">The services collection to add to</param>
        /// <param name="configuration">The app configuration </param>
        public static void AddMSGraphService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebOptions>(configuration);
            services.AddSingleton<IMSGraphService, MSGraphService>();
        }
    }
}