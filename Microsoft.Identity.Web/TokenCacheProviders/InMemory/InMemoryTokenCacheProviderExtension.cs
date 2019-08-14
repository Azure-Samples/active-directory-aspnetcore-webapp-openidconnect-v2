// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Identity.Web.TokenCacheProviders.InMemory
{
    /// <summary>
    /// Extension class used to add an in-memory token cache serializer to MSAL
    /// </summary>
    public static class InMemoryTokenCacheProviderExtension
    {
        /// <summary>Adds both the app and per-user in-memory token caches.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">The MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        /// <returns></returns>
        public static IServiceCollection AddInMemoryTokenCaches(
            this IServiceCollection services,
            MsalMemoryTokenCacheOptions cacheOptions = null)
        {
            var memoryCacheoptions = (cacheOptions == null)
                ? new MsalMemoryTokenCacheOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(14)
                }
                : cacheOptions;

            AddInMemoryAppTokenCache(services, memoryCacheoptions);
            AddInMemoryPerUserTokenCache(services, memoryCacheoptions);
            return services;
        }

        /// <summary>Adds the in-memory based application token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">The MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        public static IServiceCollection AddInMemoryAppTokenCache(
            this IServiceCollection services,
            MsalMemoryTokenCacheOptions cacheOptions)
        {
            services.AddMemoryCache();

            services.AddSingleton<IMsalAppTokenCacheProvider>(factory =>
            {
                var memoryCache = factory.GetRequiredService<IMemoryCache>();
                var optionsMonitor = factory.GetRequiredService<IOptionsMonitor<AzureADOptions>>();

                return new MsalAppMemoryTokenCacheProvider(memoryCache, cacheOptions, optionsMonitor);
            });

            return services;
        }

        /// <summary>Adds the in-memory based per user token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">The MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        /// <returns></returns>
        public static IServiceCollection AddInMemoryPerUserTokenCache(
            this IServiceCollection services,
            MsalMemoryTokenCacheOptions cacheOptions)
        {
            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            services.AddSingleton<IMsalUserTokenCacheProvider>(factory =>
            {
                var memoryCache = factory.GetRequiredService<IMemoryCache>();
                IHttpContextAccessor httpContextAccessor = factory.GetRequiredService<IHttpContextAccessor>();
                return new MsalPerUserMemoryTokenCacheProvider(memoryCache, cacheOptions, httpContextAccessor);
            });

            return services;
        }
    }
}