using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
        /// This keeps the latest copy of the token in memory to save calls to DB, if possible.
        /// </summary>
        private TokenCacheDbRecord _inMemoryCache;

        /// <summary>
        /// The data protector
        /// </summary>
        private readonly IDataProtector _dataProtector;


        protected override async Task<byte[]> ReadCacheBytesAsync(string cacheKey)
        {
            if (_inMemoryCache == null) // first time access
            {
                _inMemoryCache = GetLatestRecordQuery(cacheKey).FirstOrDefault();
            }
            else
            {
                // retrieve last written record from the DB
                var lastwriteInDb = GetLatestRecordQuery(cacheKey).Select(n => n.LastWrite).FirstOrDefault();

                // if the persisted copy is newer than the in-memory copy
                if (lastwriteInDb > _inMemoryCache.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    _inMemoryCache = GetLatestRecordQuery(cacheKey).FirstOrDefault();
                }
            }

            // Send data to the TokenCache instance
            return (_inMemoryCache == null) ? null : _dataProtector.Unprotect(_inMemoryCache.CacheBits);
        }

        protected override Task RemoveKeyAsync(string cacheKey)
        {
            var cacheEntries = _tokenCacheDb.Records.Where(c => c.CacheKey == cacheKey);
            _tokenCacheDb.Records.RemoveRange(cacheEntries);
            _tokenCacheDb.SaveChanges();
            return Task.CompletedTask;
        }

        protected override async Task WriteCacheBytesAsync(string cacheKey, byte[] bytes)
        {
            if (_inMemoryCache == null)
            {
                _inMemoryCache = new TokenCacheDbRecord
                {
                    CacheKey = cacheKey
                };
            }

            _inMemoryCache.CacheBits = _dataProtector.Protect(bytes);
            _inMemoryCache.LastWrite = DateTime.Now;

            try
            {
                // Update the DB and the lastwrite
                _tokenCacheDb.Entry(_inMemoryCache).State = _inMemoryCache.TokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
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
