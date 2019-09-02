// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders.Session
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by Http session.
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
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalAppSessionTokenCacheProvider : IMsalAppTokenCacheProvider
    {
        /// <summary>
        /// The application cache key
        /// </summary>
        internal string _appCacheId;

        /// <summary>
        /// The HTTP context being used by this app
        /// </summary>
        internal HttpContext HttpContext { get { return httpContextAccessor.HttpContext; } }

        /// <summary>
        /// HTTP context accessor
        /// </summary>
        internal IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The duration till the tokens are kept in memory cache. In production, a higher value , upto 90 days is recommended.
        /// </summary>
        private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddHours(12);

        private static readonly ReaderWriterLockSlim s_sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// The App's whose cache we are maintaining.
        /// </summary>
        private readonly string _appId;

        /// <summary>Initializes a new instance of the <see cref="MSALAppSessionTokenCacheProvider"/> class.</summary>
        /// <param name="azureAdOptionsAccessor">The azure ad options accessor.</param>
        /// <exception cref="ArgumentNullException">AzureADOptions - The app token cache needs the '{nameof(AzureADOptions)}' section in configuration, populated with clientId to initialize.</exception>
        public MsalAppSessionTokenCacheProvider(
            IOptionsMonitor<AzureADOptions> azureAdOptionsAccessor,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            if (azureAdOptionsAccessor.CurrentValue == null && string.IsNullOrWhiteSpace(azureAdOptionsAccessor.CurrentValue.ClientId))
            {
                throw new ArgumentNullException(nameof(AzureADOptions), $"The app token cache needs the '{nameof(AzureADOptions)}' section in configuration, populated with clientId to initialize.");
            }

            _appId = azureAdOptionsAccessor.CurrentValue.ClientId;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        public void Initialize(ITokenCache tokenCache)
        {
            _appCacheId = _appId + "_AppTokenCache";

            tokenCache.SetBeforeAccessAsync(AppTokenCacheBeforeAccessNotificationAsync);
            tokenCache.SetAfterAccessAsync(AppTokenCacheAfterAccessNotificationAsync);
            tokenCache.SetBeforeWrite(AppTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a SessionCache ,whose methods are threads safe, we need not to do anything in this handler.
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear()
        {
            s_sessionLock.EnterWriteLock();

            try
            {
                Debug.WriteLine($"INFO: Clearing session {HttpContext.Session.Id}, cacheId {_appCacheId}");

                // Reflect changes in the persistent store
                HttpContext.Session.Remove(_appCacheId);
                HttpContext.Session.CommitAsync().Wait();
            }
            finally
            {
                s_sessionLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private async Task AppTokenCacheBeforeAccessNotificationAsync(TokenCacheNotificationArgs args)
        {
            await HttpContext.Session.LoadAsync().ConfigureAwait(false);

            s_sessionLock.EnterReadLock();
            try
            {
                byte[] blob;
                if (HttpContext.Session.TryGetValue(_appCacheId, out blob))
                {
                    Debug.WriteLine($"INFO: Deserializing session {HttpContext.Session.Id}, cacheId {_appCacheId}");
                    args.TokenCache.DeserializeMsalV3(blob, shouldClearExistingCache: true);
                }
                else
                {
                    Debug.WriteLine($"INFO: cacheId {_appCacheId} not found in session {HttpContext.Session.Id}");
                }
            }
            finally
            {
                s_sessionLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private async Task AppTokenCacheAfterAccessNotificationAsync(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                s_sessionLock.EnterWriteLock();

                try
                {
                    Debug.WriteLine($"INFO: Serializing session {HttpContext.Session.Id}, cacheId {_appCacheId}");

                    // Reflect changes in the persistent store
                    byte[] blob = args.TokenCache.SerializeMsalV3();
                    HttpContext.Session.Set(_appCacheId, blob);
                    await HttpContext.Session.CommitAsync().ConfigureAwait(false);
                }
                finally
                {
                    s_sessionLock.ExitWriteLock();
                }
            }
        }
    }
}