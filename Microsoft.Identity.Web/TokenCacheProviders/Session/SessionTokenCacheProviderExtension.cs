// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Microsoft.Identity.Web.TokenCacheProviders.Session
{
    /// <summary>
    /// Extension class to add a session token cache serializer to MSAL
    /// </summary>
    public static class SessionTokenCacheProviderExtension
    {
        /// <summary>Adds both App and per-user session token caches.</summary>
        /// For this session cache to work effectively the aspnetcore session has to be configured properly.
        /// The latest guidance is provided at https://docs.microsoft.com/aspnet/core/fundamentals/app-state
        ///
        /// // In the method - public void ConfigureServices(IServiceCollection services) in startup.cs, add the following
        /// services.AddSession(option =>
        /// {
        ///	    option.Cookie.IsEssential = true;
        /// });
        ///
        /// In the method - public void Configure(IApplicationBuilder app, IHostingEnvironment env) in startup.cs, add the following
        ///
        /// app.UseSession(); // Before UseMvc()
        ///
        /// <param name="services">The services collection to add to.</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionTokenCaches(this IServiceCollection services)
        {
            // Add session if you are planning to use session based token cache
            var ISessionStoreservice = services.FirstOrDefault(x => x.ServiceType.Name == "ISessionStore");

            // If not added already
            if (ISessionStoreservice == null)
            {
                services.AddSession(option =>
                {
                    option.Cookie.IsEssential = true;
                });
            }
            else
            {
                // If already added, ensure the options are set to use Cookies
                services.Configure<SessionOptions>(option =>
                {
                    option.Cookie.IsEssential = true;
                });
            }

            AddSessionAppTokenCache(services);
            AddSessionPerUserTokenCache(services);

            return services;
        }

        /// <summary>Adds the Http session based application token cache to the service collection.</summary>
        /// For this session cache to work effectively the aspnetcore session has to be configured properly.
        /// The latest guidance is provided at https://docs.microsoft.com/aspnet/core/fundamentals/app-state
        ///
        /// // In the method - public void ConfigureServices(IServiceCollection services) in startup.cs, add the following
        /// services.AddSession(option =>
        /// {
        ///	    option.Cookie.IsEssential = true;
        /// });
        ///
        /// In the method - public void Configure(IApplicationBuilder app, IHostingEnvironment env) in startup.cs, add the following
        ///
        /// app.UseSession(); // Before UseMvc()
        ///
        /// <param name="services">The services collection to add to.</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionAppTokenCache(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IMsalAppTokenCacheProvider, MsalAppSessionTokenCacheProvider>();
            return services;
        }

        /// <summary>Adds the http session based per user token cache to the service collection.</summary>
        /// For this session cache to work effectively the aspnetcore session has to be configured properly.
        /// The latest guidance is provided at https://docs.microsoft.com/aspnet/core/fundamentals/app-state
        ///
        /// // In the method - public void ConfigureServices(IServiceCollection services) in startup.cs, add the following
        /// services.AddSession(option =>
        /// {
        ///	    option.Cookie.IsEssential = true;
        /// });
        ///
        /// In the method - public void Configure(IApplicationBuilder app, IHostingEnvironment env) in startup.cs, add the following
        ///
        /// app.UseSession(); // Before UseMvc()
        ///
        /// <param name="services">The services collection to add to.</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionPerUserTokenCache(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSession(option =>
            { option.Cookie.IsEssential = true; }
            );
            services.AddScoped<IMsalUserTokenCacheProvider, MsalPerUserSessionTokenCacheProvider>();
            return services;
        }
    }
}