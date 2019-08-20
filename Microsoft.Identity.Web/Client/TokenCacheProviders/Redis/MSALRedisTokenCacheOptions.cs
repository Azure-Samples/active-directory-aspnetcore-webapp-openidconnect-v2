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

using Microsoft.Extensions.Caching.StackExchangeRedis;
using System;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// MSAL's Redis token cache options
    /// </summary>
    public class MSALRedisTokenCacheOptions
    {
        /// <summary>
        /// Gets or sets the value of The fixed date and time at which the cache entry will expire..
        /// The duration till the tokens are kept in distribution cache. In production, a higher value , upto 90 days is recommended.
        /// </summary>
        /// <value>
        /// The AbsoluteExpiration value.
        /// </value>
        public TimeSpan SlidingExpiration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the Redis Connection string
        /// </summary>
        /// <value>
        /// The connectionstring for Redis.
        /// </value>
        public Action<RedisCacheOptions> RedisCacheOptions
        {
            get;
            set;
        }

        public MSALRedisTokenCacheOptions(Action<RedisCacheOptions> RedisCacheOptions)
        {
            this.SlidingExpiration = TimeSpan.FromHours(12);
            this.RedisCacheOptions = RedisCacheOptions;
        }
    }
}