// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extensions around ClaimsPrincipal.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the Account identifier for an MSAL.NET account from a <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal</param>
        /// <returns>A string corresponding to an account identifier as defined in <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></returns>
        public static string GetMsalAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = claimsPrincipal.GetObjectId();
            string nameIdentifierId = claimsPrincipal.GetNameIdentifierId();
            string tenantId = claimsPrincipal.GetTenantId();
            string userFlowId = claimsPrincipal.GetUserFlowId();

            if (!string.IsNullOrWhiteSpace(nameIdentifierId) && !string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(userFlowId))
            {
                // B2C pattern: {oid}-{tfp}.{tid}
                return $"{nameIdentifierId}-{userFlowId}.{tenantId}";
            }
            else if (!string.IsNullOrWhiteSpace(userObjectId) && !string.IsNullOrWhiteSpace(tenantId))
            {
                // AAD pattern: {oid}.{tid}
                return $"{userObjectId}.{tenantId}";
            }

            return null;
        }

        /// <summary>
        /// Gets the unique object ID associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">the <see cref="ClaimsPrincipal"/> from which to retrieve the unique object ID</param>
        /// <remarks>This method returns the object ID both in case the developer has enabled or not claims mapping</remarks>
        /// <returns>Unique object ID of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetObjectId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = claimsPrincipal.FindFirstValue(ClaimConstants.Oid);
            if (string.IsNullOrEmpty(userObjectId))
            {
                userObjectId = claimsPrincipal.FindFirstValue(ClaimConstants.ObjectId);
            }
            return userObjectId;
        }

        /// <summary>
        /// Gets the Tenant ID associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">the <see cref="ClaimsPrincipal"/> from which to retrieve the tenant ID</param>
        /// <returns>Tenant ID of the identity, or <c>null</c> if it cannot be found</returns>
        /// <remarks>This method returns the tenant ID both in case the developer has enabled or not claims mapping</remarks>
        public static string GetTenantId(this ClaimsPrincipal claimsPrincipal)
        {
            string tenantId = claimsPrincipal.FindFirstValue(ClaimConstants.Tid);
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = claimsPrincipal.FindFirstValue(ClaimConstants.TenantId);
            }
            return tenantId;
        }

        /// <summary>
        /// Gets the login-hint associated with a <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">Identity for which to complete the login-hint</param>
        /// <returns>login-hint for the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetLoginHint(this ClaimsPrincipal claimsPrincipal)
        {
            return GetDisplayName(claimsPrincipal);
        }

        /// <summary>
        /// Gets the domain-hint associated with an identity
        /// </summary>
        /// <param name="claimsPrincipal">Identity for which to compute the domain-hint</param>
        /// <returns>domain-hint for the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetDomainHint(this ClaimsPrincipal claimsPrincipal)
        {
            // Tenant for MSA accounts
            const string msaTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

            var tenantId = GetTenantId(claimsPrincipal);
            string domainHint = string.IsNullOrWhiteSpace(tenantId)
                ? null
                : tenantId.Equals(msaTenantId, StringComparison.OrdinalIgnoreCase) ? "consumers" : "organizations";

            return domainHint;
        }

        /// <summary>
        /// Get the display name for the signed-in user, from the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">Claims about the user/account</param>
        /// <returns>A string containing the display name for the user, as determined by Azure AD (v1.0) and Microsoft identity platform (v2.0) tokens,
        /// or <c>null</c> if the claims cannot be found</returns>
        /// <remarks>See https://docs.microsoft.com/azure/active-directory/develop/id-tokens#payload-claims </remarks>
        public static string GetDisplayName(this ClaimsPrincipal claimsPrincipal)
        {
            // Use the claims in a Microsoft identity platform token first
            string displayName = claimsPrincipal.FindFirstValue(ClaimConstants.PreferredUserName);

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            // Otherwise fall back to the claims in an Azure AD v1.0 token
            displayName = claimsPrincipal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            // Finally falling back to name
            return claimsPrincipal.FindFirstValue(ClaimConstants.Name);
        }
        
        /// <summary>
        /// Gets the user flow id associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">the <see cref="ClaimsPrincipal"/> from which to retrieve the user flow id</param>
        /// <returns>User Flow Id of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetUserFlowId(this ClaimsPrincipal claimsPrincipal)
        {
            string userFlowId = claimsPrincipal.FindFirstValue(ClaimConstants.Tfp);
            return userFlowId;
        }

        /// <summary>
        /// Gets the NameIdentifierId associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">the <see cref="ClaimsPrincipal"/> from which to retrieve the sub claim</param>
        /// <returns>Name identifier ID (sub) of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetNameIdentifierId(this ClaimsPrincipal claimsPrincipal)
        {
            string sub = claimsPrincipal.FindFirstValue(ClaimConstants.Sub);
            if (string.IsNullOrEmpty(sub))
            {
                sub = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            return sub;
        }
    }
}
