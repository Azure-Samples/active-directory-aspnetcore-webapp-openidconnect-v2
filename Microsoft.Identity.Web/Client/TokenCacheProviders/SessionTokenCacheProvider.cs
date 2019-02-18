using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// Provides an implementation of <see cref="ITokenCacheProvider"/> for a cookie based token cache implementation
    /// </summary>
    public class SessionBasedTokenCacheProvider : ITokenCacheProvider
    {
        SessionTokenCacheProvider helper;

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
            helper = new SessionTokenCacheProvider(userId, httpContext);
            return helper.GetMsalCacheInstance();
        }
    }

    public class SessionTokenCacheProvider
    {
        private static readonly ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private string UserId;
        private string CacheId;
        private ISession session;

        private TokenCache cache = new TokenCache();

        public SessionTokenCacheProvider(string userId, HttpContext httpcontext)
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
            if (args.HasStateChanged) Persist();
        }
    }
}
