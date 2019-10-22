// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    public class MsalSqlTokenCacheProvider : MsalAbstractTokenCacheProvider
    {
        public MsalSqlTokenCacheProvider(IHttpContextAccessor httpContextAccessor, TokenCacheDbContext tokenCacheDbContext, IOptions<AzureADOptions> azureAdOptionsAccessor, IDataProtectionProvider protectionProvider) :
            base(azureAdOptionsAccessor, httpContextAccessor)
        {
            _dataProtector = protectionProvider.CreateProtector("MSAL");
            _tokenCacheDb = tokenCacheDbContext;
        }
        /// <summary>
        /// The EF's DBContext object to be used to read and write from the Sql server database.
        /// </summary>
        private readonly TokenCacheDbContext _tokenCacheDb;

        /// <summary>
        /// This keeps the latest copy of the token in memory ..
        /// </summary>
        private TokenCacheDbRecord _cacheDbRecord;

        /// <summary>
        /// The data protector
        /// </summary>
        private readonly IDataProtector _dataProtector;


        protected override Task<byte[]> ReadCacheBytesAsync(string cacheKey)
        {
            try
            {
                _cacheDbRecord = GetLatestRecordQuery(cacheKey).FirstOrDefault();
            }
            catch (SqlException ex) when (ex.Message == "Invalid object name 'Records'")
            {
                // Microsoft.Identity.Web changed from several tables (AppTokenCache, UserTokenCache) to one table (record)
                // If you care about the cache, you can migrate them, otherwise re-create the database
                throw new Exception("You need to migrate the AppTokenCache and UserTokenCache tables to Records, or run SqlTokenCacheProviderExtension.CreateTokenCachingTablesInSqlDatabase() to create the database", ex);
            }
            catch (SqlException ex) when (ex.Message.StartsWith("Cannot open database"))
            {
                throw new Exception("You need to run SqlTokenCacheProviderExtension.CreateTokenCachingTablesInSqlDatabase() to create the database", ex);
            }

            // Send data to the TokenCache instance
            return Task.FromResult((_cacheDbRecord == null) ? null : _dataProtector.Unprotect(_cacheDbRecord.CacheBits));
        }

        protected override async Task RemoveKeyAsync(string cacheKey)
        {
            var cacheEntries = _tokenCacheDb.Records.Where(c => c.CacheKey == cacheKey);
            _tokenCacheDb.Records.RemoveRange(cacheEntries);
            await _tokenCacheDb.SaveChangesAsync();
        }

        protected override async Task WriteCacheBytesAsync(string cacheKey, byte[] bytes)
        {
            if (_cacheDbRecord == null)
            {
                _cacheDbRecord = new TokenCacheDbRecord
                {
                    CacheKey = cacheKey
                };
            }

            _cacheDbRecord.CacheBits = _dataProtector.Protect(bytes);
            _cacheDbRecord.LastWrite = DateTime.Now;

            try
            {
                // Update the DB and the lastwrite
                _tokenCacheDb.Entry(_cacheDbRecord).State = _cacheDbRecord.TokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
                _tokenCacheDb.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Record already updated on a different thread, so just read the updated record
                await ReadCacheBytesAsync(cacheKey).ConfigureAwait(false);
            }
        }

        private IOrderedQueryable<TokenCacheDbRecord> GetLatestRecordQuery(string cacheKey)
        {
            return _tokenCacheDb.Records.Where(c => c.CacheKey == cacheKey).OrderByDescending(d => d.LastWrite);
        }
    }
}
