namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    /// <summary>
    /// Contains the cache settings for the memory cache instance used by this app to cache group memberships
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Sets how long the cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        public string SlidingExpirationInSeconds { get; set; }

        /// <summary>
        /// Sets an absolute expiration date for the cache entry.
        /// </summary>
        public string AbsoluteExpirationInSeconds { get; set; }
    }
}