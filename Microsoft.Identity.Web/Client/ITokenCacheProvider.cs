using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Identity.Web.Client
{
    /// <summary>
    /// Token cache provider service.
    /// </summary>
    public interface ITokenCacheProvider
    {
        /// <summary>
        /// Enables the token cache serialization
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize</param>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="claimsPrincipal">Claims principal (account) for which to retrieve the cache</param>
        void EnableSerialization(ITokenCache tokenCache, HttpContext httpContext, ClaimsPrincipal claimsPrincipal);
    }
}
