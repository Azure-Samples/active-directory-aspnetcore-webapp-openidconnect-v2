// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extension methods dealing with IAccount instances.
    /// </summary>
    public static class AccountExtensions
    {
        /// <summary>
        /// Creates the <see cref="ClaimsPrincipal"/> from the values found 
        /// in an <see cref="IAccount"/>
        /// </summary>
        /// <param name="account">The IAccount instance</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> built from IAccount</returns>
        public static ClaimsPrincipal ToClaimsPrincipal(this IAccount account)
        {
            if (account != null)
            {
                return new ClaimsPrincipal(
                    new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimConstants.Oid, account.HomeAccountId.ObjectId),
                        new Claim(ClaimConstants.Tid, account.HomeAccountId.TenantId),
                        new Claim(ClaimTypes.Upn, account.Username)
                    })
                );
            }

            return null;
        }
    }
}
