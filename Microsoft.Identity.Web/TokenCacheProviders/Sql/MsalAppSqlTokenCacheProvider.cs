// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by Sql server and Entity Framework
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalAppSqlTokenCacheProvider : IMsalAppTokenCacheProvider
    {
        /// <summary>
        /// The EF's DBContext object to be used to read and write from the Sql server database.
        /// </summary>
        private readonly TokenCacheDbContext _tokenCacheDb;

        /// <summary>
        /// This keeps the latest copy of the token in memory to save calls to DB, if possible.
        /// </summary>
        private AppTokenCache _inMemoryCache;

        /// <summary>
        /// The data protector
        /// </summary>
        private readonly IDataProtector _dataProtector;

        /// <summary>
        /// Once a app obtains a token, this is populated and used for caching queries et al. Contains the App's AppId/ClientID as obtained from the Azure AD portal
        /// </summary>
        internal string _activeClientId;

        /// <summary>Initializes a new instance of the <see cref="MsalAppSqlTokenCacheProvider"/> class.</summary>
        /// <param name="tokenCacheDbContext">The TokenCacheDbContext DbContext to read and write from Sql server.</param>
        /// <param name="azureAdOptionsAccessor"></param>
        /// <param name="protectionProvider">The data protection provider. Requires the caller to have used serviceCollection.AddDataProtection();</param>
        public MsalAppSqlTokenCacheProvider(TokenCacheDbContext tokenCacheDbContext, IOptionsMonitor<AzureADOptions> azureAdOptionsAccessor, IDataProtectionProvider protectionProvider)
        {
            if (protectionProvider == null)
            {
                throw new ArgumentNullException(nameof(protectionProvider), $"The app token cache needs an {nameof(IDataProtectionProvider)} to operate. Please use 'serviceCollection.AddDataProtection();' to add the data protection provider to the service collection");
            }

            if (azureAdOptionsAccessor.CurrentValue == null && string.IsNullOrWhiteSpace(azureAdOptionsAccessor.CurrentValue.ClientId))
            {
                throw new ArgumentNullException(nameof(protectionProvider), $"The app token cache needs {nameof(AzureADOptions)}, populated with both Sql connection string and clientId to initialize.");
            }

            _dataProtector = protectionProvider.CreateProtector("MSAL");
            _tokenCacheDb = tokenCacheDbContext;
            _activeClientId = azureAdOptionsAccessor.CurrentValue.ClientId;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext)
        {
            tokenCache.SetBeforeAccess(AppTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(AppTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(AppTokenCacheBeforeWriteNotification);
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
            if (args.HasStateChanged && !string.IsNullOrWhiteSpace(_activeClientId))
            {
                if (_inMemoryCache == null)
                {
                    _inMemoryCache = new AppTokenCache
                    {
                        ClientID = _activeClientId
                    };
                }

                _inMemoryCache.CacheBits = _dataProtector.Protect(args.TokenCache.SerializeMsalV3());
                _inMemoryCache.LastWrite = DateTime.Now;

                try
                {
                    // Update the DB and the lastwrite
                    _tokenCacheDb.Entry(_inMemoryCache).State = _inMemoryCache.AppTokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
                    _tokenCacheDb.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Record already updated on a different thread, so just read the updated record
                    ReadCacheForSignedInApp(args);
                }
            }
        }

        /// <summary>
        /// Clears all tokens belonging to the currently signed-in client Id from database
        /// </summary>
        public void Clear()
        {
            var cacheEntries = _tokenCacheDb.AppTokenCache.Where(c => c.ClientID == _activeClientId);
            _tokenCacheDb.AppTokenCache.RemoveRange(cacheEntries);
            _tokenCacheDb.SaveChanges();
        }

        /// <summary>
        /// Right before it reads the cache, a call is made to BeforeAccess notification. Here, you have the opportunity of retrieving your persisted cache blob
        /// from the Sql database. We pick it from the database, save it in the in-memory copy, and pass it to the base class by calling the Deserialize().
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            ReadCacheForSignedInApp(args);
        }

        /// <summary>
        /// Reads the cache data from the backend database.
        /// </summary>
        private void ReadCacheForSignedInApp(TokenCacheNotificationArgs args)
        {
            if (_inMemoryCache == null) // first time access
            {
                _inMemoryCache = GetLatestAppRecordQuery().FirstOrDefault();
            }
            else
            {
                // retrieve last written record from the DB
                var lastwriteInDb = GetLatestAppRecordQuery().Select(n => n.LastWrite).FirstOrDefault();

                // if the persisted copy is newer than the in-memory copy
                if (lastwriteInDb > _inMemoryCache.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    _inMemoryCache = GetLatestAppRecordQuery().FirstOrDefault();
                }
            }

            // Send data to the TokenCache instance
            args.TokenCache.DeserializeMsalV3((_inMemoryCache == null) ? null : _dataProtector.Unprotect(_inMemoryCache.CacheBits), shouldClearExistingCache: true);
        }

        private IOrderedQueryable<AppTokenCache> GetLatestAppRecordQuery()
        {
            return _tokenCacheDb.AppTokenCache.Where(c => c.ClientID == _activeClientId).OrderByDescending(d => d.LastWrite);
        }
    }
}