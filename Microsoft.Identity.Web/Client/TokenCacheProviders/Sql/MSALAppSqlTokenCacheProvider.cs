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

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by Sql server and Entity Framework
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MSALAppSqlTokenCacheProvider : IMSALAppTokenCacheProvider
    {
        /// <summary>
        /// The EF's DBContext object to be used to read and write from the Sql server database.
        /// </summary>
        private TokenCacheDbContext TokenCacheDb;

        /// <summary>
        /// This keeps the latest copy of the token in memory to save calls to DB, if possible.
        /// </summary>
        private AppTokenCache InMemoryCache;

        /// <summary>
        /// The data protector
        /// </summary>
        private IDataProtector DataProtector;

        /// <summary>
        /// Once a app obtains a token, this is populated and used for caching queries et al. Contains the App's AppId/ClientID as obtained from the Azure AD portal
        /// </summary>
        internal string ActiveClientId;

        /// <summary>Initializes a new instance of the <see cref="EFMSALAppTokenCache"/> class.</summary>
        /// <param name="tokenCacheDbContext">The TokenCacheDbContext DbContext to read and write from Sql server.</param>
        /// <param name="azureAdOptionsAccessor"></param>
        /// <param name="protectionProvider">The data protection provider. Requires the caller to have used serviceCollection.AddDataProtection();</param>
        public MSALAppSqlTokenCacheProvider(TokenCacheDbContext tokenCacheDbContext, IOptionsMonitor<AzureADOptions> azureAdOptionsAccessor, IDataProtectionProvider protectionProvider)
        {
            if (protectionProvider == null)
            {
                throw new ArgumentNullException(nameof(protectionProvider), $"The app token cache needs an {nameof(IDataProtectionProvider)} to operate. Please use 'serviceCollection.AddDataProtection();' to add the data protection provider to the service collection");
            }

            if (azureAdOptionsAccessor.CurrentValue == null && string.IsNullOrWhiteSpace(azureAdOptionsAccessor.CurrentValue.ClientId))
            {
                throw new ArgumentNullException(nameof(protectionProvider), $"The app token cache needs {nameof(AzureADOptions)}, populated with both Sql connection string and clientId to initialize.");
            }

            this.DataProtector = protectionProvider.CreateProtector("MSAL");
            this.TokenCacheDb = tokenCacheDbContext;
            this.ActiveClientId = azureAdOptionsAccessor.CurrentValue.ClientId;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext)
        {
            tokenCache.SetBeforeAccess(this.AppTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(this.AppTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(this.AppTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a Rowversion for concurrency, we need not to do anything in this handler.
        }

        /// <summary>
        /// Raised AFTER MSAL added the new token in its in-memory copy of the cache.
        /// This notification is called every time MSAL accessed the cache, not just when a write took place:
        /// If MSAL's current operation resulted in a cache change, the property TokenCacheNotificationArgs.HasStateChanged will be set to true.
        /// If that is the case, we call the TokenCache.Serialize() to get a binary blob representing the latest cache content – and persist it.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed, i.e. new token obtained
            if (args.HasStateChanged && !string.IsNullOrWhiteSpace(this.ActiveClientId))
            {
                if (this.InMemoryCache == null)
                {
                    this.InMemoryCache = new AppTokenCache
                    {
                        ClientID = this.ActiveClientId
                    };
                }

                this.InMemoryCache.CacheBits = this.DataProtector.Protect(args.TokenCache.SerializeMsalV3());
                this.InMemoryCache.LastWrite = DateTime.Now;

                try
                {
                    // Update the DB and the lastwrite
                    this.TokenCacheDb.Entry(InMemoryCache).State = InMemoryCache.AppTokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
                    this.TokenCacheDb.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Record already updated on a different thread, so just read the updated record
                    this.ReadCacheForSignedInApp(args);
                }
            }
        }

        /// <summary>
        /// Clears all tokens belonging to the currently signed-in client Id from database
        /// </summary>
        public void Clear()
        {
            var cacheEntries = this.TokenCacheDb.AppTokenCache.Where(c => c.ClientID == this.ActiveClientId);
            this.TokenCacheDb.AppTokenCache.RemoveRange(cacheEntries);
            this.TokenCacheDb.SaveChanges();
        }

        /// <summary>
        /// Right before it reads the cache, a call is made to BeforeAccess notification. Here, you have the opportunity of retrieving your persisted cache blob
        /// from the Sql database. We pick it from the database, save it in the in-memory copy, and pass it to the base class by calling the Deserialize().
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            this.ReadCacheForSignedInApp(args);
        }

        /// <summary>
        /// Reads the cache data from the backend database.
        /// </summary>
        private void ReadCacheForSignedInApp(TokenCacheNotificationArgs args)
        {
            if (this.InMemoryCache == null) // first time access
            {
                this.InMemoryCache = GetLatestAppRecordQuery().FirstOrDefault();
            }
            else
            {
                // retrieve last written record from the DB
                var lastwriteInDb = GetLatestAppRecordQuery().Select(n => n.LastWrite).FirstOrDefault();

                // if the persisted copy is newer than the in-memory copy
                if (lastwriteInDb > this.InMemoryCache.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    this.InMemoryCache = GetLatestAppRecordQuery().FirstOrDefault();
                }
            }

            // Send data to the TokenCache instance
            args.TokenCache.DeserializeMsalV3((this.InMemoryCache == null) ? null : this.DataProtector.Unprotect(this.InMemoryCache.CacheBits), shouldClearExistingCache: true);
        }

        private IOrderedQueryable<AppTokenCache> GetLatestAppRecordQuery()
        {
            return this.TokenCacheDb.AppTokenCache.Where(c => c.ClientID == this.ActiveClientId).OrderByDescending(d => d.LastWrite);
        }
    }

    /// <summary>
    /// Represents an app's token cache entry in database
    /// </summary>
    public class AppTokenCache
    {
        [Key]
        public int AppTokenCacheId { get; set; }

        /// <summary>
        /// The Appid or ClientId of the app
        /// </summary>
        public string ClientID { get; set; }

        public byte[] CacheBits { get; set; }

        public DateTime LastWrite { get; set; }

        /// <summary>
        /// Provided here as a precaution against concurrent updates by multiple threads.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}