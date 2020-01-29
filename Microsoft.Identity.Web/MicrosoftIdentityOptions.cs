﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Microsoft.Identity.Web
{
    public class MicrosoftIdentityOptions : OpenIdConnectOptions
    {
        /// <summary>
        /// Gets or sets the Azure Active Directory instance.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the domain of the Azure Active Directory tenant.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the edit profile policy name.
        /// </summary>
        public string EditProfilePolicyId { get; set; }

        /// <summary>
        /// Gets or sets the sign up or sign in policy name.
        /// </summary>
        public string SignUpSignInPolicyId { get; set; }

        /// <summary>
        /// Gets or sets the reset password policy id.
        /// </summary>
        public string ResetPasswordPolicyId { get; set; }

        /// <summary>
        /// Gets the default user flow (which is signUpsignIn).
        /// </summary>
        public string DefaultUserFlow => SignUpSignInPolicyId;

        internal bool IsB2C { get { return !string.IsNullOrWhiteSpace(DefaultUserFlow); } }
    }
}
