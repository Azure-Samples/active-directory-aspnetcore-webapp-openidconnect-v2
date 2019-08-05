// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Identity.Web.InstanceDiscovery
{
    /// <summary>
    /// Model class to hold information parsed from the Azure AD issuer endpoint
    /// </summary>
    internal class IssuerMetadata
    {
        /// <summary>
        /// Tenant discovery endpoint
        /// </summary>
        [JsonProperty(PropertyName = "tenant_discovery_endpoint")]
        public string TenantDiscoveryEndpoint { get; set; }

        /// <summary>
        /// API Version
        /// </summary>
        [JsonProperty(PropertyName = "api-version")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// List of metadata associated with the endpoint
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public List<Metadata> Metadata { get; set; }
    }
}