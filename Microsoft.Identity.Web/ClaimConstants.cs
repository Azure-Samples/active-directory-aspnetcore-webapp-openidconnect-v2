// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Constants for claim types.
    /// </summary>
    public static class ClaimConstants
    {
        public const string Name = "name";
        public const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string Oid = "oid";
        public const string PreferredUserName = "preferred_username";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string Tid = "tid";
        public const string Scope = "http://schemas.microsoft.com/identity/claims/scope";
        public const string Roles = "roles";
    }
}
