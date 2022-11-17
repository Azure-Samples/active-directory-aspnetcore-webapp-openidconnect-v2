namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    /// <summary>
    /// Contains all the caching settings available in this application.
    /// </summary>
    public class CacheSettings
    {
        public string SlidingExpirationInSeconds { get; set; }
        public string AbsoluteExpirationInSeconds { get; set; }
    }
}