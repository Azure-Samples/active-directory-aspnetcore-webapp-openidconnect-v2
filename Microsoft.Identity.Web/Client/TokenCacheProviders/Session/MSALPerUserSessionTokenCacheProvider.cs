/*
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
*/

using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// This is a MSAL's TokenCache implementation for one user. It uses Http session as a backend store
    /// </summary>
    public class MSALPerUserSessionTokenCacheProvider : IMSALUserTokenCacheProvider
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Once the user signes in, this will not be null and can be ontained via a call to ClaimsPrincipal.Current
        /// </summary>
        internal ClaimsPrincipal SignedInUser;

        /// <summary>
        /// The HTTP context being used by this app
        /// </summary>
        internal HttpContext HttpContext = null;

        /// <summary>
        /// The internal handle to the client's instance of the Cache
        /// </summary>
        private ITokenCache UserTokenCache;

        /// <summary>Initializes a new instance of the <see cref="MSALPerUserSessionTokenCache"/> class.</summary>
        public MSALPerUserSessionTokenCacheProvider()
        {
        }

        /// <summary>Initializes the cache instance</summary>
        /// <param name="tokenCache">The ITokenCache passed through the constructor</param>
        /// <param name="httpcontext">The current HttpContext</param>
        /// <param name="user">The signed in user's ClaimPrincipal, could be null.
        /// If the calling app has it available, then it should pass it themselves.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext, ClaimsPrincipal user)
        {
            this.HttpContext = httpcontext;

            this.UserTokenCache = tokenCache;

            this.UserTokenCache.SetBeforeAccess(this.UserTokenCacheBeforeAccessNotification);
            this.UserTokenCache.SetAfterAccess(this.UserTokenCacheAfterAccessNotification);
            this.UserTokenCache.SetBeforeWrite(this.UserTokenCacheBeforeWriteNotification);

            if (user == null)
            {
                // No users signed in yet, so we return
                return;
            }

            this.SignedInUser = user;
            this.LoadUserTokenCacheFromSession();
        }

        /// <summary>
        /// Loads the user token cache from http session.
        /// </summary>
        private void LoadUserTokenCacheFromSession()
        {
            this.HttpContext.Session.LoadAsync().Wait();

            string cacheKey = this.GetSignedInUsersUniqueId();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            SessionLock.EnterReadLock();
            try
            {
                byte[] blob;
                if (this.HttpContext.Session.TryGetValue(cacheKey, out blob))
                {
                    Debug.WriteLine($"INFO: Deserializing session {this.HttpContext.Session.Id}, cacheId {cacheKey}");
                    this.UserTokenCache.DeserializeMsalV3(blob);
                }
                else
                {
                    Debug.WriteLine($"INFO: cacheId {cacheKey} not found in session {this.HttpContext.Session.Id}");
                }
            }
            finally
            {
                SessionLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Persists the user token blob to the Http session.
        /// </summary>
        private void PersistUserTokenCache()
        {
            string cacheKey = this.GetSignedInUsersUniqueId();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            SessionLock.EnterWriteLock();

            try
            {
                Debug.WriteLine($"INFO: Serializing session {this.HttpContext.Session.Id}, cacheId {cacheKey}");

                // Reflect changes in the persistent store
                byte[] blob = this.UserTokenCache.SerializeMsalV3();
                this.HttpContext.Session.Set(cacheKey, blob);
                this.HttpContext.Session.CommitAsync().Wait();
            }
            finally
            {
                SessionLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear()
        {
            string cacheKey = this.GetSignedInUsersUniqueId();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            SessionLock.EnterWriteLock();

            try
            {
                Debug.WriteLine($"INFO: Clearing session {this.HttpContext.Session.Id}, cacheId {cacheKey}");

                // Reflect changes in the persistent store
                this.HttpContext.Session.Remove(cacheKey);
                this.HttpContext.Session.CommitAsync().Wait();
            }
            finally
            {
                SessionLock.ExitWriteLock();
            }

            // Nulls the currently deserialized instance
            this.LoadUserTokenCacheFromSession();
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
            this.SetSignedInUserFromNotificationArgs(args);

            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                this.PersistUserTokenCache();
            }
        }

        /// <summary>
        /// To keep the cache, ClaimsPrincipal and Sql in sync, we ensure that the user's object Id we obtained by MSAL after
        /// successful sign-in is set as the key for the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void SetSignedInUserFromNotificationArgs(TokenCacheNotificationArgs args)
        {
            if (this.SignedInUser == null && args.Account != null)
            {
                this.SignedInUser = args.Account.ToClaimsPrincipal();
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            this.LoadUserTokenCacheFromSession();
        }

        /// <summary>
        /// Explores the Claims of a signed-in user (if available) to populate the unique Id of this cache's instance.
        /// </summary>
        /// <returns>The signed in user's object.tenant Id , if available in the ClaimsPrincipal.Current instance</returns>
        internal string GetSignedInUsersUniqueId()
        {
            if (this.SignedInUser != null)
            {
                return this.SignedInUser.GetMsalAccountId();
            }
            return null;
        }
    }
}