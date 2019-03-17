﻿/************************************************************************************************
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext to cache data.
    /// </summary>
    /// <seealso cref="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization"/>
    public class MSALAppMemoryTokenCacheProvider : IMSALAppTokenCacheProvider
    {
        /// <summary>
        /// The application cache key
        /// </summary>
        internal string AppCacheId;

        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        internal IMemoryCache memoryCache;

        /// <summary>
        /// The duration till the tokens are kept in memory cache. In production, a higher value , upto 90 days is recommended.
        /// </summary>
        private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddHours(12);

        /// <summary>
        /// The internal handle to the client's instance of the Cache
        /// </summary>
        private ITokenCache ApptokenCache;

        private IConfiguration Configuration;

        public MSALAppMemoryTokenCacheProvider(IConfiguration configuration, IMemoryCache cache)
        {
            this.Configuration = configuration;
            this.memoryCache = cache;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext)
        {
            AzureADOptions azureAdOptions = new AzureADOptions();
            this.Configuration.Bind("AzureAD", azureAdOptions);

            this.AppCacheId = azureAdOptions.ClientId + "_AppTokenCache";

            this.ApptokenCache = tokenCache;
            this.ApptokenCache.SetBeforeAccess(this.AppTokenCacheBeforeAccessNotification);
            this.ApptokenCache.SetAfterAccess(this.AppTokenCacheAfterAccessNotification);
            this.ApptokenCache.SetBeforeWrite(this.AppTokenCacheBeforeWriteNotification);

            this.LoadAppTokenCacheFromMemory();
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a MemoryCache ,whose methods are threads safe, we need not to do anything in this handler.
        }

        /// <summary>
        /// Loads the application's token from memory cache.
        /// </summary>
        private void LoadAppTokenCacheFromMemory()
        {
            byte[] tokenCacheBytes = (byte[])this.memoryCache.Get(this.AppCacheId);
            this.ApptokenCache.DeserializeMsalV3(tokenCacheBytes);
        }

        /// <summary>
        /// Persists the application's token to the cache.
        /// </summary>
        private void PersistAppTokenCache()
        {
            // Reflect changes in the persistence store
            this.memoryCache.Set(this.AppCacheId, this.ApptokenCache.SerializeMsalV3(), this.cacheDuration);
        }

        /// <summary>
        /// Clears the token cache for this app
        /// </summary>
        public void Clear()
        {
            this.memoryCache.Remove(this.AppCacheId);

            // Nulls the currently deserialized instance
            this.LoadAppTokenCacheFromMemory();
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            this.LoadAppTokenCacheFromMemory();
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                this.PersistAppTokenCache();
            }
        }
    }
}