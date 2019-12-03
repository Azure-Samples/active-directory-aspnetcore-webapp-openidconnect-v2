// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Claims;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Factory class to create ClaimsPrincipal objects.
    /// </summary>
    public static class ClaimsPrincipalFactory
    {
        /// <summary>
        /// Instantiate a ClaimsPrincipal from an account objectId and tenantId. This can
        /// be useful when the Web app subscribes to another service on behalf of the user
        /// and then is called back by a notification where the user is identified by his tenant
        /// id and object id (like in Microsoft Graph Web Hooks)
        /// </summary>
        /// <param name="tenantId">Tenant Id of the account</param>
        /// <param name="objectId">Object Id of the account in this tenant ID</param>
        /// <returns>A ClaimsPrincipal containing these two claims</returns>
        ///
        /// <example>
        /// <code>
        /// private async Task GetChangedMessagesAsync(IEnumerable&lt;Notification&gt; notifications)
        /// {
        ///  foreach (var notification in notifications)
        ///  {
        ///   SubscriptionStore subscription =
        ///           subscriptionStore.GetSubscriptionInfo(notification.SubscriptionId);
        ///  HttpContext.User = ClaimsPrincipalExtension.FromTenantIdAndObjectId(subscription.TenantId,
        ///                                                                      subscription.UserId);
        ///  string accessToken = await tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(scopes);
        /// </code>
        /// </example>
        public static ClaimsPrincipal FromTenantIdAndObjectId(string tenantId, string objectId)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimConstants.Tid, tenantId),
                    new Claim(ClaimConstants.Oid, objectId)
                })
            );
        }

    }
}
