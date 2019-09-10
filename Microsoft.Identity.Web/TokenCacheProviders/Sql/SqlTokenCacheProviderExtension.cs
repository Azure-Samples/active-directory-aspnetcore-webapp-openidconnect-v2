// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// Extension class to add a SQL Server based token cache serializer to MSAL.NET
    /// </summary>
    public static class SqlTokenCacheProviderExtension
    {
        /// <summary>Adds the app and per user SQL Server token caches.</summary>
        /// <param name="configuration">The configuration instance from where this method pulls the connection string to the Sql database.</param>
        /// <param name="sqlTokenCacheOptions">The MSALSqlTokenCacheOptions is used by the caller to specify the Sql connection string</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlTokenCaches(
            this IServiceCollection services,
            MsalSqlTokenCacheOptions sqlTokenCacheOptions)
        {
            AddSqlAppTokenCache(services, sqlTokenCacheOptions);
            AddSqlPerUserTokenCache(services, sqlTokenCacheOptions);
            return services;
        }

        /// <summary>Adds the Sql Server based application token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="sqlTokenCacheOptions">The MSALSqlTokenCacheOptions is used by the caller to specify the Sql connection string</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlAppTokenCache(
            this IServiceCollection services,
            MsalSqlTokenCacheOptions sqlTokenCacheOptions)
        {
            services.AddDataProtection();

            services.AddDbContext<TokenCacheDbContext>(options =>
                options.UseSqlServer(sqlTokenCacheOptions.SqlConnectionString));

            services.AddScoped<IMsalAppTokenCacheProvider,MsalAppSqlTokenCacheProvider>();

            return services;
        }

        /// <summary>Adds the Sql Server based per user token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <param name="sqlTokenCacheOptions">The MSALSqlTokenCacheOptions is used by the caller to specify the Sql connection string</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlPerUserTokenCache(
            this IServiceCollection services,
            MsalSqlTokenCacheOptions sqlTokenCacheOptions)
        {
            // To share protected payloads among apps, configure SetApplicationName in each app with the same value. 
            // https://docs.microsoft.com/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-2.2#setapplicationname
            services.AddDataProtection()
                .SetApplicationName(sqlTokenCacheOptions.ApplicationName);

            services.AddDbContext<TokenCacheDbContext>(options =>
                options.UseSqlServer(sqlTokenCacheOptions.SqlConnectionString));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IMsalUserTokenCacheProvider,MsalPerUserSqlTokenCacheProvider>();
            return services;
        }

        /// <summary>A one time method that can be used to create the tables required for token caching in a Sql server database</summary>
        /// <param name="sqlTokenCacheOptions">The SQL token cache options containing the connection string to the Sql Server database.</param>
        /// <remarks>In production scenarios, the database  will most probably be already present.</remarks>
        public static void CreateTokenCachingTablesInSqlDatabase(MsalSqlTokenCacheOptions sqlTokenCacheOptions)
        {
            var tokenCacheDbContextBuilder = new DbContextOptionsBuilder<TokenCacheDbContext>();
            tokenCacheDbContextBuilder.UseSqlServer(sqlTokenCacheOptions.SqlConnectionString);

            var tokenCacheDbContextForCreation = new TokenCacheDbContext(tokenCacheDbContextBuilder.Options);
            tokenCacheDbContextForCreation.Database.EnsureCreated();
        }
    }
}