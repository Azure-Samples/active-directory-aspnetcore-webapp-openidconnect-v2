using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Identity.Web.Client
{
    using TokenCacheProviders;
    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implementation service
    /// </summary>
    public static class InMemoryTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddInMemoryTokenCache(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            // Token acquisition service
            services.AddSingleton<ITokenCacheProvider, InMemoryTokenCacheProvider>();
            return services;
        }
    }
}
