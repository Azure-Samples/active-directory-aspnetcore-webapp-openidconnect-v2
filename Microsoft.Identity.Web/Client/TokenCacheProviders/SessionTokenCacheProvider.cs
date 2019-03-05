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
        /// Enables the token cache serialization
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize</param>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="claimsPrincipal">Information about the user for which to serialize the cache. Can be 
        /// null for an application token cache</param>
        public void EnableSerialization(ITokenCache tokenCache, HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            string userId = claimsPrincipal.GetMsalAccountId() ?? "_Application_";
            helper = new SessionTokenCacheProvider(tokenCache, userId, httpContext);
            helper.GetMsalCacheInstance();
        }
    }

    public class SessionTokenCacheProvider
    {
        private static readonly ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private string UserId;
        private string CacheId;
        private ISession session;

        private ITokenCache cache;

        public SessionTokenCacheProvider(ITokenCache tokenCache, string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            this.cache = tokenCache;
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            session = httpcontext.Session;
        }

        public ITokenCache GetMsalCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
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
                    cache.DeserializeMsalV3(blob);
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
                byte[] blob = cache.SerializeMsalV3();
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
