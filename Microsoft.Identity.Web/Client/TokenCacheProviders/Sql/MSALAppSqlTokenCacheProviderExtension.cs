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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    public static class MSALAppSqlTokenCacheProviderExtension
    {
        /// <summary>Adds the app and per user SQL token caches.</summary>
        /// <param name="configuration">The configuration instance from where this method pulls the connection string to the Sql database.</param>
        /// <param name="sqlTokenCacheOptions">The MSALSqlTokenCacheOptions is used by the caller to specify the Sql connection string</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlTokenCaches(this IServiceCollection services, MSALSqlTokenCacheOptions sqlTokenCacheOptions)
        {
            AddSqlAppTokenCache(services, sqlTokenCacheOptions);
            AddSqlPerUserTokenCache(services, sqlTokenCacheOptions);
            return services;
        }

        /// <summary>Adds the Sql Server based application token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="sqlTokenCacheOptions">The MSALSqlTokenCacheOptions is used by the caller to specify the Sql connection string</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlAppTokenCache(this IServiceCollection services, MSALSqlTokenCacheOptions sqlTokenCacheOptions)
        {
            // Uncomment the following lines to create the database. In production scenarios, the database
            // will most probably be already present.
/*
            var tokenCacheDbContextBuilder = new DbContextOptionsBuilder<TokenCacheDbContext>();
            tokenCacheDbContextBuilder.UseSqlServer(sqlTokenCacheOptions.SqlConnectionString);

            var tokenCacheDbContextForCreation = new TokenCacheDbContext(tokenCacheDbContextBuilder.Options);
            tokenCacheDbContextForCreation.Database.EnsureCreated();
*/
            services.AddDataProtection();

            services.AddDbContext<TokenCacheDbContext>(options =>
                options.UseSqlServer(sqlTokenCacheOptions.SqlConnectionString));

            services.AddScoped<IMSALAppTokenCacheProvider>(factory =>
            {
                var dpprovider = factory.GetRequiredService<IDataProtectionProvider>();
                var tokenCacheDbContext = factory.GetRequiredService<TokenCacheDbContext>();
                var optionsMonitor = factory.GetRequiredService<IOptionsMonitor<AzureADOptions>>();

                return new MSALAppSqlTokenCacheProvider(tokenCacheDbContext, optionsMonitor, dpprovider);
            });

            return services;
        }

        /// <summary>Adds the Sql Server based per user token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="sqlTokenCacheOptions">The MSALSqlTokenCacheOptions is used by the caller to specify the Sql connection string</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlPerUserTokenCache(this IServiceCollection services, MSALSqlTokenCacheOptions sqlTokenCacheOptions)
        {
            // Uncomment the following lines to create the database. In production scenarios, the database
            // will most probably be already present.
            //var tokenCacheDbContextBuilder = new DbContextOptionsBuilder<TokenCacheDbContext>();
            //tokenCacheDbContextBuilder.UseSqlServer(sqlTokenCacheOptions.SqlConnectionString);

            //var tokenCacheDbContext = new TokenCacheDbContext(tokenCacheDbContextBuilder.Options);
            //tokenCacheDbContext.Database.EnsureCreated();

            services.AddDataProtection();

            services.AddDbContext<TokenCacheDbContext>(options =>
                options.UseSqlServer(sqlTokenCacheOptions.SqlConnectionString));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IMSALUserTokenCacheProvider>(factory =>
            {
                var dpprovider = factory.GetRequiredService<IDataProtectionProvider>();
                var tokenCacheDbContext = factory.GetRequiredService<TokenCacheDbContext>();
                var httpcontext = factory.GetRequiredService<IHttpContextAccessor>();

                return new MSALPerUserSqlTokenCacheProvider(tokenCacheDbContext, dpprovider, httpcontext);
            });

            return services;
        }
    }
}