/*
 The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    public static class MSALAppMemoryTokenCacheProviderExtension
    {
        /// <summary>Adds both the app and per-user in-memory token caches.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">the MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        /// <returns></returns>
        public static IServiceCollection AddInMemoryTokenCaches(this IServiceCollection services, MSALMemoryTokenCacheOptions cacheOptions = null)
        {
            var memoryCacheoptions = (cacheOptions == null) ? new MSALMemoryTokenCacheOptions { AbsoluteExpiration = DateTimeOffset.Now.AddDays(14) }
            : cacheOptions;

            AddInMemoryAppTokenCache(services, memoryCacheoptions);
            AddInMemoryPerUserTokenCache(services, memoryCacheoptions);
            return services;
        }


        /// <summary>Adds the in-memory based application token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">the MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        public static IServiceCollection AddInMemoryAppTokenCache(this IServiceCollection services, MSALMemoryTokenCacheOptions cacheOptions)
        {
            services.AddMemoryCache();

            services.AddSingleton<IMSALAppTokenCacheProvider>(factory =>
            {
                var memoryCache = factory.GetRequiredService<IMemoryCache>();
                var optionsMonitor = factory.GetRequiredService<IOptionsMonitor<AzureADOptions>>();

                return new MSALAppMemoryTokenCacheProvider(memoryCache, cacheOptions, optionsMonitor);
            });

            return services;
        }

        /// <summary>Adds the in-memory based per user token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="cacheOptions">the MSALMemoryTokenCacheOptions allows the caller to set the token cache expiration</param>
        /// <returns></returns>
        public static IServiceCollection AddInMemoryPerUserTokenCache(this IServiceCollection services, MSALMemoryTokenCacheOptions cacheOptions)
        {
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSingleton<IMSALUserTokenCacheProvider>(factory =>
            {
                var memoryCache = factory.GetRequiredService<IMemoryCache>();
                IHttpContextAccessor httpContextAccessor = factory.GetRequiredService<IHttpContextAccessor>();
                return new MSALPerUserMemoryTokenCacheProvider(memoryCache, cacheOptions, httpContextAccessor);
            });

            return services;
        }
    }
}