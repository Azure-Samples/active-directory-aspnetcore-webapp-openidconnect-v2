// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Linq;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// This is a MSAL's TokenCache implementation for one user. It uses Sql server as the persistence store and uses the Entity Framework to read and write to that database.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalPerUserSqlTokenCacheProvider : IMsalUserTokenCacheProvider
    {
        /// <summary>
        /// The EF's DBContext object to be used to read and write from the Sql server database.
        /// </summary>
        private readonly TokenCacheDbContext _tokenCacheDb;

        /// <summary>
        /// This keeps the latest copy of the token in memory to save calls to DB, if possible.
        /// </summary>
        private UserTokenCache _inMemoryCache;

        /// <summary>
        /// The data protector
        /// </summary>
        private readonly IDataProtector _dataProtector;

        private IHttpContextAccessor httpContextAccesssor;

        /// <summary>Initializes a new instance of the <see cref="MsalPerUserSqlTokenCacheProvider"/> class.</summary>
        /// <param name="protectionProvider">The data protection provider. Requires the caller to have used serviceCollection.AddDataProtection();</param>
        /// <param name="tokenCacheDbContext">The DbContext to the database where tokens will be cached.</param>
        /// <param name="httpContext">The current HttpContext that has a user signed-in</param>
        public MsalPerUserSqlTokenCacheProvider(
            TokenCacheDbContext tokenCacheDbContext,
            IDataProtectionProvider protectionProvider,
            IHttpContextAccessor httpContext)
            : this(tokenCacheDbContext, protectionProvider)
        {
            this.httpContextAccesssor = httpContext;
        }

        /// <summary>Initializes a new instance of the <see cref="MsalPerUserSqlTokenCacheProvider"/> class.</summary>
        /// <param name="tokenCacheDbContext">The token cache database context.</param>
        /// <param name="protectionProvider">The protection provider.</param>
        /// <param name="user">The current user .</param>
        /// <exception cref="ArgumentNullException">protectionProvider - The app token cache needs an {nameof(IDataProtectionProvider)}</exception>
        public MsalPerUserSqlTokenCacheProvider(
            TokenCacheDbContext tokenCacheDbContext,
            IDataProtectionProvider protectionProvider)
        {
            if (protectionProvider == null)
            {
                throw new ArgumentNullException(nameof(protectionProvider), $"The app token cache needs an {nameof(IDataProtectionProvider)} to operate. Please use 'serviceCollection.AddDataProtection();' to add the data protection provider to the service collection");
            }

            _dataProtector = protectionProvider.CreateProtector("MSAL");
            _tokenCacheDb = tokenCacheDbContext;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        public void Initialize(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(UserTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(UserTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(UserTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a Rowversion for concurrency, we need not to do anything in this handler.
        }

        /// <summary>
        /// Right before it reads the cache, a call is made to BeforeAccess notification. Here, you have the opportunity of retrieving your persisted cache blob
        /// from the Sql database. We pick it from the database, save it in the in-memory copy, and pass it to the base class by calling the Deserialize().
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            ReadCacheForSignedInUser(args);
        }

        /// <summary>
        /// Raised AFTER MSAL added the new token in its in-memory copy of the cache.
        /// This notification is called every time MSAL accessed the cache, not just when a write took place:
        /// If MSAL's current operation resulted in a cache change, the property TokenCacheNotificationArgs.HasStateChanged will be set to true.
        /// If that is the case, we call the TokenCache.Serialize() to get a binary blob representing the latest cache content – and persist it.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            string accountId = httpContextAccesssor.HttpContext.User.GetMsalAccountId();

            // if state changed, i.e. new token obtained
            if (args.HasStateChanged && !string.IsNullOrWhiteSpace(accountId))
            {
                if (_inMemoryCache == null)
                {
                    _inMemoryCache = new UserTokenCache
                    {
                        WebUserUniqueId = accountId
                    };
                }

                _inMemoryCache.CacheBits = _dataProtector.Protect(args.TokenCache.SerializeMsalV3());
                _inMemoryCache.LastWrite = DateTime.Now;

                try
                {
                    // Update the DB and the lastwrite
                    _tokenCacheDb.Entry(_inMemoryCache).State = _inMemoryCache.UserTokenCacheId == 0
                        ? EntityState.Added
                        : EntityState.Modified;

                    _tokenCacheDb.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Record already updated on a different thread, so just read the updated record
                    ReadCacheForSignedInUser(args);
                }
            }
        }

        /// <summary>
        /// Reads the cache data from the backend database.
        /// </summary>
        private void ReadCacheForSignedInUser(TokenCacheNotificationArgs args)
        {
            string accountId = httpContextAccesssor.HttpContext.User.GetMsalAccountId();
            if (_inMemoryCache == null) // first time access
            {
                _inMemoryCache = GetLatestUserRecordQuery(accountId).FirstOrDefault();
            }
            else
            {
                // retrieve last written record from the DB
                var lastwriteInDb = GetLatestUserRecordQuery(accountId).Select(n => n.LastWrite).FirstOrDefault();

                // if the persisted copy is newer than the in-memory copy
                if (lastwriteInDb > _inMemoryCache.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    _inMemoryCache = GetLatestUserRecordQuery(accountId).FirstOrDefault();
                }
            }

            // Send data to the TokenCache instance
            args.TokenCache.DeserializeMsalV3((_inMemoryCache == null) ? null : _dataProtector.Unprotect(_inMemoryCache.CacheBits), shouldClearExistingCache: true);
        }

        /// <summary>
        /// Clears the TokenCache's copy and the database copy of this user's cache.
        /// </summary>
        public void Clear(string accountId)
        {
            // Delete from DB
            var cacheEntries = _tokenCacheDb.UserTokenCache.Where(c => c.WebUserUniqueId == accountId);
            _tokenCacheDb.UserTokenCache.RemoveRange(cacheEntries);
            _tokenCacheDb.SaveChanges();
        }

        private IOrderedQueryable<UserTokenCache> GetLatestUserRecordQuery(string accountId)
        {
            return _tokenCacheDb.UserTokenCache
                                    .Where(c => c.WebUserUniqueId == accountId)
                                    .OrderByDescending(d => d.LastWrite);
        }
    }
}