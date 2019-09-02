// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Identity.Web.TokenCacheProviders.Session
{
    /// <summary>
    /// This is a MSAL's TokenCache implementation for one user. It uses Http session as a persistence store
    /// </summary>
    /// For this session cache to work effectively the aspnetcore session has to be configured properly.
    /// The latest guidance is provided at https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state
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
    public class MsalPerUserSessionTokenCacheProvider : IMsalUserTokenCacheProvider
    {
        private static readonly ReaderWriterLockSlim s_sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// The HTTP context being used by this app
        /// </summary>
        internal HttpContext HttpContext { get { return httpContextAccessor.HttpContext; } }

        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>Initializes a new instance of the <see cref="MsalPerUserSessionTokenCacheProvider"/> class.</summary>
        public MsalPerUserSessionTokenCacheProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>Initializes the cache instance</summary>
        /// <param name="tokenCache">The <see cref="ITokenCache"/> passed through the constructor</param>
        public void Initialize(ITokenCache tokenCache)
        {
            if (tokenCache == null)
            {
                throw new ArgumentNullException(nameof(tokenCache));
            }
            tokenCache.SetBeforeAccess(this.UserTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(this.UserTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(this.UserTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear(string accountId)
        {
            string cacheKey = accountId;

            s_sessionLock.EnterWriteLock();

            try
            {
                Debug.WriteLine($"INFO: Clearing session {HttpContext.Session.Id}, cacheId {cacheKey}");

                // Reflect changes in the persistent store
                HttpContext.Session.Remove(cacheKey);
                HttpContext.Session.CommitAsync().Wait();
            }
            finally
            {
                s_sessionLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we obtain and release lock right before and after we read the Http session, we need not do anything here.
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                string cacheKey = httpContextAccessor.HttpContext.User.GetMsalAccountId();

                if (string.IsNullOrWhiteSpace(cacheKey))
                    return;

                s_sessionLock.EnterWriteLock();

                try
                {
                    Debug.WriteLine($"INFO: Serializing session {HttpContext.Session.Id}, cacheId {cacheKey}");

                    // Reflect changes in the persistent store
                    byte[] blob = args.TokenCache.SerializeMsalV3();
                    HttpContext.Session.Set(cacheKey, blob);
                    HttpContext.Session.CommitAsync().Wait();
                }
                finally
                {
                    s_sessionLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            HttpContext.Session.LoadAsync().Wait();
            string cacheKey = httpContextAccessor.HttpContext.User.GetMsalAccountId();
            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            s_sessionLock.EnterReadLock();
            try
            {
                if (HttpContext.Session.TryGetValue(cacheKey, out byte[] blob))
                {
                    Debug.WriteLine($"INFO: Deserializing session {HttpContext.Session.Id}, cacheId {cacheKey}");
                    args.TokenCache.DeserializeMsalV3(blob, shouldClearExistingCache: true);
                }
                else
                {
                    Debug.WriteLine($"INFO: cacheId {cacheKey} not found in session {HttpContext.Session.Id}");
                }
            }
            finally
            {
                s_sessionLock.ExitReadLock();
            }
        }
    }
}