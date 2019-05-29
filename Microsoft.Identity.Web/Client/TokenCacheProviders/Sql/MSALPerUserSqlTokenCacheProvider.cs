/************************************************************************************************
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
***********************************************************************************************/

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// This is a MSAL's TokenCache implementation for one user. It uses Sql server as a backend store and uses the Entity Framework to read and write to that database.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MSALPerUserSqlTokenCacheProvider : IMSALUserTokenCacheProvider
    {
        /// <summary>
        /// The EF's DBContext object to be used to read and write from the Sql server database.
        /// </summary>
        private TokenCacheDbContext TokenCacheDb;

        /// <summary>
        /// This keeps the latest copy of the token in memory to save calls to DB, if possible.
        /// </summary>
        private UserTokenCache InMemoryCache;

        /// <summary>
        /// The data protector
        /// </summary>
        private IDataProtector DataProtector;

        private IHttpContextAccessor httpContextAccesssor;

        /// <summary>Initializes a new instance of the <see cref="EFMSALPerUserTokenCache"/> class.</summary>
        /// <param name="protectionProvider">The data protection provider. Requires the caller to have used serviceCollection.AddDataProtection();</param>
        /// <param name="tokenCacheDbContext">The DbContext to the database where tokens will be cached.</param>
        /// <param name="httpContext">The current HttpContext that has a user signed-in</param>
        public MSALPerUserSqlTokenCacheProvider(TokenCacheDbContext tokenCacheDbContext, IDataProtectionProvider protectionProvider, IHttpContextAccessor httpContext)
            : this(tokenCacheDbContext, protectionProvider, httpContext?.HttpContext?.User)
        {
            this.httpContextAccesssor = httpContext;
        }

        /// <summary>Initializes a new instance of the <see cref="MSALPerUserSqlTokenCacheProvider"/> class.</summary>
        /// <param name="tokenCacheDbContext">The token cache database context.</param>
        /// <param name="protectionProvider">The protection provider.</param>
        /// <param name="user">The current user .</param>
        /// <exception cref="ArgumentNullException">protectionProvider - The app token cache needs an {nameof(IDataProtectionProvider)}</exception>
        public MSALPerUserSqlTokenCacheProvider(TokenCacheDbContext tokenCacheDbContext, IDataProtectionProvider protectionProvider, ClaimsPrincipal user)
        {
            if (protectionProvider == null)
            {
                throw new ArgumentNullException(nameof(protectionProvider), $"The app token cache needs an {nameof(IDataProtectionProvider)} to operate. Please use 'serviceCollection.AddDataProtection();' to add the data protection provider to the service collection");
            }

            this.DataProtector = protectionProvider.CreateProtector("MSAL");
            this.TokenCacheDb = tokenCacheDbContext;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established. Not needed by all providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext, ClaimsPrincipal user)
        {
            tokenCache.SetBeforeAccess(this.UserTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(this.UserTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(this.UserTokenCacheBeforeWriteNotification);
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
            this.ReadCacheForSignedInUser(args);
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
            string accountId = args.Account?.HomeAccountId?.Identifier;
            if (string.IsNullOrEmpty(accountId))
            {
                accountId = httpContextAccesssor.HttpContext.User.GetMsalAccountId();
            }

            // if state changed, i.e. new token obtained
            if (args.HasStateChanged && !string.IsNullOrWhiteSpace(accountId))
            {
                if (this.InMemoryCache == null)
                {
                    this.InMemoryCache = new UserTokenCache
                    {
                        WebUserUniqueId = accountId
                    };
                }

                this.InMemoryCache.CacheBits = this.DataProtector.Protect(args.TokenCache.SerializeMsalV3());
                this.InMemoryCache.LastWrite = DateTime.Now;

                try
                {
                    // Update the DB and the lastwrite
                    this.TokenCacheDb.Entry(InMemoryCache).State = InMemoryCache.UserTokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
                    this.TokenCacheDb.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Record already updated on a different thread, so just read the updated record
                    this.ReadCacheForSignedInUser(args);
                }
            }
        }

        /// <summary>
        /// Reads the cache data from the backend database.
        /// </summary>
        private void ReadCacheForSignedInUser(TokenCacheNotificationArgs args)
        {
            string accountId = args.Account?.HomeAccountId?.Identifier;
            if (string.IsNullOrEmpty(accountId))
            {
                accountId = httpContextAccesssor.HttpContext.User.GetMsalAccountId();
            }
            if (this.InMemoryCache == null) // first time access
            {
                this.InMemoryCache = GetLatestUserRecordQuery(accountId).FirstOrDefault();
            }
            else
            {
                // retrieve last written record from the DB
                var lastwriteInDb = GetLatestUserRecordQuery(accountId).Select(n => n.LastWrite).FirstOrDefault();

                // if the persisted copy is newer than the in-memory copy
                if (lastwriteInDb > InMemoryCache.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    this.InMemoryCache = GetLatestUserRecordQuery(accountId).FirstOrDefault();
                }
            }

            // Send data to the TokenCache instance
            args.TokenCache.DeserializeMsalV3((InMemoryCache == null) ? null : this.DataProtector.Unprotect(InMemoryCache.CacheBits), shouldClearExistingCache: true);
        }

        /// <summary>
        /// Clears the TokenCache's copy and the database copy of this user's cache.
        /// </summary>
        public void Clear(string accountId)
        {
            // Delete from DB
            var cacheEntries = this.TokenCacheDb.UserTokenCache.Where(c => c.WebUserUniqueId == accountId);
            this.TokenCacheDb.UserTokenCache.RemoveRange(cacheEntries);
            this.TokenCacheDb.SaveChanges();
        }

        private IOrderedQueryable<UserTokenCache> GetLatestUserRecordQuery(string accountId)
        {
            return this.TokenCacheDb.UserTokenCache
                                    .Where(c => c.WebUserUniqueId == accountId)
                                    .OrderByDescending(d => d.LastWrite);
        }
    }

    /// <summary>
    /// Represents a user's token cache entry in database
    /// </summary>
    public class UserTokenCache
    {
        [Key]
        public int UserTokenCacheId { get; set; }

        /// <summary>
        /// The objectId of the signed-in user's object in Azure AD
        /// </summary>
        public string WebUserUniqueId { get; set; }

        public byte[] CacheBits { get; set; }

        public DateTime LastWrite { get; set; }

        /// <summary>
        /// Provided here as a precaution against concurrent updates by multiple threads.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}