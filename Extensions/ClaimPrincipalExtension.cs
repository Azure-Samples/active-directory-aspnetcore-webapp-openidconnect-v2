using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    public static class ClaimsPrincipalExtension
    {
        /// <summary>
        /// Get the Account identifier for an MSAL.NET account from a ClaimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal</param>
        /// <returns>A string corresponding to an account identifier as defined in <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></returns>
        public static string GetAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (string.IsNullOrEmpty(userObjectId))
            {
                userObjectId = claimsPrincipal.FindFirst("oid")?.Value;
            }
            string tenantId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = claimsPrincipal.FindFirst("tid")?.Value;
            }

            if (string.IsNullOrWhiteSpace(userObjectId)) // TODO: find a better typed exception
                throw new Exception("Missing claim 'http://schemas.microsoft.com/identity/claims/objectidentifier' or 'oid' ");

            if (string.IsNullOrWhiteSpace(tenantId))
                throw new Exception("Missing claim 'http://schemas.microsoft.com/identity/claims/tenantid' or 'tid' ");

            string accountId = userObjectId + "." + tenantId;
            return accountId;
        }
    }
}
