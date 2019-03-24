using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Microsoft.Identity.Web
{
    public static class ClaimsPrincipalExtension
    {
        /// <summary>
        /// Get the Account identifier for an MSAL.NET account from a ClaimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal</param>
        /// <returns>A string corresponding to an account identifier as defined in <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></returns>
        public static string GetMsalAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = GetObjectId(claimsPrincipal);
            string tenantId = GetTenantId(claimsPrincipal);

            if (!string.IsNullOrWhiteSpace(userObjectId) && !string.IsNullOrWhiteSpace(tenantId))
            {
                return $"{userObjectId}.{tenantId}";
            }

            return null;
        }

        /// <summary>
        /// Get the unique object ID associated with the claimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal from which to retrieve the unique object id</param>
        /// <returns>Unique object ID of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetObjectId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = claimsPrincipal.FindFirstValue(ClaimConstants.ObjectId);
            if (string.IsNullOrEmpty(userObjectId))
            {
                userObjectId = claimsPrincipal.FindFirstValue("oid");
            }

            return userObjectId;
        }

        /// <summary>
        /// Tenant ID of the identity
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal from which to retrieve the tenant id</param>
        /// <returns>Tenant ID of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetTenantId(this ClaimsPrincipal claimsPrincipal)
        {
            string tenantId = claimsPrincipal.FindFirstValue(ClaimConstants.TenantId);
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = claimsPrincipal.FindFirstValue("tid");
            }

            return tenantId;
        }

        /// <summary>
        /// Gets the login-hint associated with an identity
        /// </summary>
        /// <param name="claimsPrincipal">Identity for which to compte the login-hint</param>
        /// <returns>login-hint for the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetLoginHint(this ClaimsPrincipal claimsPrincipal)
        {
            return GetDisplayName(claimsPrincipal);
        }

        /// <summary>
        /// Gets the domain-hint associated with an identity
        /// </summary>
        /// <param name="claimsPrincipal">Identity for which to compte the domain-hint</param>
        /// <returns>domain-hint for the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetDomainHint(this ClaimsPrincipal claimsPrincipal)
        {
            // Tenant for MSA accounts
            const string msaTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

            var tenantId = GetTenantId(claimsPrincipal);
            string domainHint = string.IsNullOrWhiteSpace(tenantId) ? null :
                tenantId == msaTenantId ? "consumers" : "organizations";
            return domainHint;
        }

        /// <summary>
        /// Get the display name for the signed-in user, based on their claims principal
        /// </summary>
        /// <param name="claimsPrincipal">Claims about the user/account</param>
        /// <returns>A string containing the display name for the user, as brought by Azure AD v1.0 and v2.0 tokens,
        /// or <c>null</c> if the claims cannot be found</returns>
        /// <remarks>See https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens#payload-claims </remarks>
        public static string GetDisplayName(this ClaimsPrincipal claimsPrincipal)
        {
            // Attempting the claims brought by an Azure AD v2.0 token first
            string displayName = claimsPrincipal.FindFirstValue("preferred_username");

            // Otherwise falling back to the claims brought by an Azure AD v1.0 token
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
            }

            // Finally falling back to name
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue("name");
            }
            return displayName;
        }

        /// <summary>
        /// Instantiate a ClaimsPrincipal from an account objectId and tenantId. This can
        /// we useful when the Web app subscribes to another service on behalf of the user
        /// and then is called back by a notification where the user is identified by his tenant
        /// id and object id (like in Microsoft Graph Web Hooks)
        /// </summary>
        /// <param name="tenantId">Tenant Id of the account</param>
        /// <param name="objectId">Object Id of the account in this tenant ID</param>
        /// <returns>A ClaimsPrincipal containing these two claims</returns>
        /// <example>
        /// <code>
        /// private async Task GetChangedMessagesAsync(IEnumerable<Notification> notifications)
        /// {
        ///  foreach (var notification in notifications)
        ///  {
        ///   SubscriptionStore subscription =
        ///           subscriptionStore.GetSubscriptionInfo(notification.SubscriptionId);
        ///  HttpContext.User = ClaimsPrincipalExtension.FromTenantIdAndObjectId(subscription.TenantId,
        ///                                                                      subscription.UserId);
        ///  string accessToken = await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, scopes);,
        /// </code>
        /// </example>
        public static ClaimsPrincipal FromTenantIdAndObjectId(string tenantId, string objectId)
        {
            var tidClaim = new Claim("tid", tenantId);
            var oidClaim = new Claim("oid", objectId);
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaims(new Claim[] { oidClaim, tidClaim });
            var principal = new ClaimsPrincipal();
            principal.AddIdentity(claimsIdentity);
            return principal;
        }

        /// <summary>
        /// Builds a ClaimsPrincipal from an IAccount
        /// </summary>
        /// <param name="account">The IAccount instance.</param>
        /// <returns>A ClaimsPrincipal built from IAccount</returns>
        public static ClaimsPrincipal ToClaimsPrincipal(this IAccount account)
        {
            if (account != null)
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimConstants.ObjectId, account.HomeAccountId.ObjectId));
                identity.AddClaim(new Claim(ClaimConstants.TenantId, account.HomeAccountId.TenantId));
                identity.AddClaim(new Claim(ClaimTypes.Upn, account.Username));
                return new ClaimsPrincipal(identity);
            }

            return null;
        }
    }
}