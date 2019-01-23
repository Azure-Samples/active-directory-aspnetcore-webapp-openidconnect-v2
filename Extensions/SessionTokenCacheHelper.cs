using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implentation service
    /// </summary>
    public static class SessionBasedTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddSessionBasedTokenCache(this IServiceCollection services)
        {
            // Token acquisition service
            services.AddSingleton<ITokenCacheProvider, SessionBasedTokenCacheProvider>();
            return services;
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="ITokenCacheProvider"/> for a cookie based token cache implementation
    /// </summary>
    public class SessionBasedTokenCacheProvider : ITokenCacheProvider
    {
        SessionTokenCacheHelper helper;

        /// <summary>
        /// Get an MSAL.NET Token cache from the HttpContext, and possibly the AuthenticationProperties and Cookies sign-in scheme
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="authenticationProperties">Authentication properties</param>
        /// <param name="signInScheme">Sign-in scheme</param>
        /// <returns>A token cache to use in the application</returns>

        public TokenCache GetCache(HttpContext httpContext, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme)
        {
            string userId = claimsPrincipal.GetMsalAccountId();
            helper = new SessionTokenCacheHelper(userId, httpContext);
            return helper.GetMsalCacheInstance();
        }
    }

    public class SessionTokenCacheHelper
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string UserId = string.Empty;
        string CacheId = string.Empty;
        ISession session;

        TokenCache cache = new TokenCache();

        public SessionTokenCacheHelper(string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            session = httpcontext.Session;
            Load();
        }

        public TokenCache GetMsalCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return cache;
        }

        public void Load()
        {
            session.LoadAsync().Wait();

            SessionLock.EnterReadLock();
            try
            {
                byte[] blob;
                if (session.TryGetValue(CacheId, out blob))
                {
                    Debug.WriteLine($"INFO: Deserializing session {session.Id}, cacheId {CacheId}");
                    cache.Deserialize(blob);
                }
                else
                {
                    Debug.WriteLine($"INFO: cacheId {CacheId} not found in session {session.Id}");
                }
            }
            finally
            {
                SessionLock.ExitReadLock();
            }
        }

        public void Persist()
        {
            SessionLock.EnterWriteLock();

            try
            {
                Debug.WriteLine($"INFO: Serializing session {session.Id}, cacheId {CacheId}");

                // Reflect changes in the persistent store
                byte[] blob = cache.Serialize();
                session.Set(CacheId, blob);
                session.CommitAsync().Wait();
            }
            finally
            {
                SessionLock.ExitWriteLock();
            }
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
