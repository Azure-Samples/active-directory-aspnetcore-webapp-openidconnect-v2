using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Identity.Web.Client
{
    using TokenCacheProviders;

    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implentation service
    /// </summary>
    public static class CookieTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddCookieBasedTokenCache(this IServiceCollection services)
        {
            // Token acquisition service
            services.AddSingleton<ITokenCacheProvider, CookieTokenCacheProvider>();
            return services;
        }
    }

 
}
