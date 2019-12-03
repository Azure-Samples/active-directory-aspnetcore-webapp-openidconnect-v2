using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Microsoft.Identity.Web
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Keep the validated token associated with the Http request
        /// </summary>
        /// <param name="httpContext">Http context</param>
        /// <param name="token">Token to preserve after the token is validated so that
        /// it can be used in the actions</param>
        public static void StoreTokenUsedToCallWebAPI(this HttpContext httpContext, JwtSecurityToken token)
        {
            httpContext.Items.Add("JwtSecurityTokenUsedToCallWebAPI", token);
        }

        /// <summary>
        /// Get the parsed information about the token used to call the Web API
        /// </summary>
        /// <param name="httpContext">Http context associated with the current request</param>
        /// <returns><see cref="JwtSecurityToken"/> used to call the Web API</returns>
        public static JwtSecurityToken GetTokenUsedToCallWebAPI(this HttpContext httpContext)
        {
            return httpContext.Items["JwtSecurityTokenUsedToCallWebAPI"] as JwtSecurityToken;
        }
    }
}
