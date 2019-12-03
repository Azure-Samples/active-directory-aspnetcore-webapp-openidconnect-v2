// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Identity.Web.TokenCacheProviders.Distributed
{
    /// <summary>
    /// Extension class used to add an in-memory token cache serializer to MSAL
    /// </summary>
    public static class DistributedTokenCacheAdapterExtension
    {
        /// <summary>Adds both the app and per-user in-memory token caches.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">The MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        /// <returns></returns>
        public static IServiceCollection AddDistributedTokenCaches(
            this IServiceCollection services)
        {
            AddDistributedAppTokenCache(services);
            AddDistributedUserTokenCache(services);
            return services;
        }

        /// <summary>Adds the in-memory based application token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">The MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        public static IServiceCollection AddDistributedAppTokenCache(
            this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IMsalTokenCacheProvider, MsalDistributedTokenCacheAdapter>();
            return services;
        }

        /// <summary>Adds the in-memory based per user token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">The MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        /// <returns></returns>
        public static IServiceCollection AddDistributedUserTokenCache(
            this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSingleton<IMsalTokenCacheProvider, MsalDistributedTokenCacheAdapter>();
            return services;
        }
    }
}