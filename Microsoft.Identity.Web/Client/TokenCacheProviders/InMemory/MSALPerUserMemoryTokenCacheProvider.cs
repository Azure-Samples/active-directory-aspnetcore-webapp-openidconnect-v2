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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// An implementation of token cache for both Confidential and Public clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext.Session to cache data.
    /// </summary>
    /// <seealso cref="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization"/>
    public class MSALPerUserMemoryTokenCacheProvider : IMSALUserTokenCacheProvider
    {
        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        internal IMemoryCache memoryCache;

        /// <summary>
        /// The internal handle to the client's instance of the Cache
        /// </summary>
        private ITokenCache UserTokenCache;

        /// <summary>
        /// Once the user signes in, this will not be null and can be ontained via a call to Thread.CurrentPrincipal
        /// </summary>
        internal ClaimsPrincipal SignedInUser;

        private readonly MSALMemoryTokenCacheOptions CacheOptions;

        /// <summary>Initializes a new instance of the <see cref="MSALPerUserMemoryTokenCache"/> class.</summary>
        /// <param name="cache">The memory cache instance</param>
        public MSALPerUserMemoryTokenCacheProvider(IMemoryCache cache, MSALMemoryTokenCacheOptions option)
        {
            this.memoryCache = cache;

            if (option != null)
            {
                this.CacheOptions = new MSALMemoryTokenCacheOptions();
            }
            else
            {
                this.CacheOptions = option;
            }
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established. Not needed by all providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext, ClaimsPrincipal user)
        {
            this.SignedInUser = user;

            this.UserTokenCache = tokenCache;
            this.UserTokenCache.SetBeforeAccess(this.UserTokenCacheBeforeAccessNotification);
            this.UserTokenCache.SetAfterAccess(this.UserTokenCacheAfterAccessNotification);
            this.UserTokenCache.SetBeforeWrite(this.UserTokenCacheBeforeWriteNotification);

            if (this.SignedInUser == null)
            {
                // No users signed in yet, so we return
                return;
            }

            this.LoadUserTokenCacheFromMemory();
        }

        /// <summary>
        /// Explores the Claims of a signed-in user (if available) to populate the unique Id of this cache's instance.
        /// </summary>
        /// <returns>The signed in user's object.tenant Id , if available in the ClaimsPrincipal.Current instance</returns>
        internal string GetMsalAccountId()
        {
            if (this.SignedInUser != null)
            {
                return this.SignedInUser.GetMsalAccountId();
            }
            return null;
        }

        /// <summary>Loads the user token cache from memory.</summary>
        private void LoadUserTokenCacheFromMemory()
        {
            string cacheKey = this.GetMsalAccountId();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            byte[] tokenCacheBytes = (byte[])this.memoryCache.Get(this.GetMsalAccountId());
            this.UserTokenCache.DeserializeMsalV3(tokenCacheBytes);
        }

        /// <summary>
        /// Persists the user token blob to the memoryCache.
        /// </summary>
        private void PersistUserTokenCache()
        {
            // Ideally, methods that load and persist should be thread safe.MemoryCache.Get() is thread safe.
            this.memoryCache.Set(this.GetMsalAccountId(), this.UserTokenCache.SerializeMsalV3(), this.CacheOptions.AbsoluteExpiration);
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear()
        {
            this.memoryCache.Remove(this.GetMsalAccountId());

            // Nulls the currently deserialized instance
            this.LoadUserTokenCacheFromMemory();
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
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it
        /// changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            this.LoadUserTokenCacheFromMemory();
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
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
    }
}