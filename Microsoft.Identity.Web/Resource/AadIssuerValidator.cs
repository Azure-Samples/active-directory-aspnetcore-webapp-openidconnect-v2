/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Microsoft.Identity.Web.Resource
{
    /// <summary>
    /// Generic class that validates token issuer from the provided Azure AD authority
    /// </summary>
    public class AadIssuerValidator
    {
        /// <summary>
        /// A list of all Issuers across the various Azure AD instances
        /// </summary>
        private readonly SortedSet<string> _issuerAliases;
        private const string _fallBackAuthority = "https://login.microsoftonline.com/";
        private static IDictionary<string, AadIssuerValidator> _issuerValidators = new ConcurrentDictionary<string, AadIssuerValidator>();
        private static string _azureADIssuerMetadataUrl = "https://login.microsoftonline.com/common/discovery/instance?authorization_endpoint=https://login.microsoftonline.com/common/oauth2/v2.0/authorize&api-version=1.1";
        private static ConfigurationManager<IssuerMetadata> _configManager = new ConfigurationManager<IssuerMetadata>(_azureADIssuerMetadataUrl, new IssuerConfigurationRetriever());

        private AadIssuerValidator(IEnumerable<string> aliases)
        {
            _issuerAliases = new SortedSet<string>(aliases);
        }

        /// <summary>
        /// Gets a <see cref="AadIssuerValidator"/> for an authority.
        /// </summary>
        /// <param name="aadAuthority">the authority to create the validator for.</param>
        /// <returns>a <see cref="AadIssuerValidator"/> for the aadAuthority.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="aadAuthority"/> is null or empty.</exception>
        public static AadIssuerValidator GetIssuerValidator(string aadAuthority)
        {
            if (string.IsNullOrEmpty(aadAuthority))
                throw new ArgumentNullException(nameof(aadAuthority));

            if (_issuerValidators.TryGetValue(aadAuthority, out AadIssuerValidator aadIssuerValidator))
            {
                return aadIssuerValidator;
            }
            else
            {
                // In the constructor, we hit the Azure AD issuer metadata endpoint and cache the aliases. The data is cached for 24 hrs.
                var issuerMetadata = _configManager.GetConfigurationAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                string authorityHost;
                try
                {
                     authorityHost = new Uri(aadAuthority).Authority;
                }
                catch
                {
                    authorityHost = null;
                }

                // Add issuer aliases of the chosen authority
                string authority = authorityHost ?? _fallBackAuthority;
                var aliases = issuerMetadata.Metadata.Where(m => m.Aliases.Any(a => a == authority)).SelectMany(m => m.Aliases).Distinct();
                _issuerValidators[authority] = new AadIssuerValidator(aliases);
                return _issuerValidators[authority];
            }
        }

        /// <summary>
        /// Validate the issuer for multi-tenant applications of various audience (Work and School account, or Work and School accounts +
        /// Personal accounts)
        /// </summary>
        /// <param name="issuer">Issuer to validate (will be tenanted)</param>
        /// <param name="securityToken">Received Security Token</param>
        /// <param name="validationParameters">Token Validation parameters</param>
        /// <remarks>The issuer is considered as valid if it has the same http scheme and authority as the
        /// authority from the configuration file, has a tenant Id, and optionally v2.0 (this web api
        /// accepts both V1 and V2 tokens).
        /// Authority aliasing is also taken into account</remarks>
        /// <returns>The <c>issuer</c> if it's valid, or otherwise <c>SecurityTokenInvalidIssuerException</c> is thrown</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="securityToken"/> is null.</exception>
        /// <exception cref="ArgumentNullException"> if <paramref name="validationParameters"/> is null.</exception>
        /// <exception cref="SecurityTokenInvalidIssuerException">if the issuer </exception>
        public string ValidateAadIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            if (validationParameters == null)
                throw new ArgumentNullException(nameof(validationParameters));


            string tenantId = GetTenantIdFromToken(securityToken);
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new SecurityTokenInvalidIssuerException("Neither `tid` nor `tenantId` claim is present in the token obtained from Microsoft Identity Platform.");

            if (validationParameters.ValidIssuers != null)
                foreach (var validIssuer in validationParameters.ValidIssuers)
                    if (IsValidIssuer(validIssuer, tenantId))
                        return issuer;

            if (IsValidIssuer(validationParameters.ValidIssuer, tenantId))
                return issuer;

            // If a valid issuer is not found, throw
            // brentsch - todo, create a list of all the possible valid issuers in TokenValidationParameters
            throw new SecurityTokenInvalidIssuerException($"Issuer: '{issuer}', does not match any of the valid issuers provided for this application.");
        }

        private bool IsValidIssuer(string validIssuer, string tenantId)
        {
            if (string.IsNullOrEmpty(validIssuer))
                return false;

            try
            {
                var uri = new Uri(validIssuer.Replace("{tenantid}", tenantId));
                if (_issuerAliases.Contains(uri.Authority))
                {
                    string trimmedLocalPath = uri.LocalPath.Trim('/');
                    return (trimmedLocalPath == tenantId || trimmedLocalPath == $"{tenantId}/v2.0");
                }
            }
            catch
            {
                // if something faults, ignore
            }

            return false;
        }

        /// <summary>Gets the tenant id from a token.</summary>
        /// <param name="securityToken">A JWT token.</param>
        /// <returns>A string containing tenantId, if found or <see cref="string.Empty"/>.</returns>
        /// <remarks>Only <see cref="JwtSecurityToken"/> and <see cref="JsonWebToken"/> are acceptable types.</remarks>
        private static string GetTenantIdFromToken(SecurityToken securityToken)
        {
            if (securityToken is JwtSecurityToken jwtSecurityToken)
            {
                if (jwtSecurityToken.Payload.TryGetValue(ClaimConstants.Tid, out object tenantId))
                    return tenantId as string;
            }

            // brentsch - todo, TryGetPayloadValue is available in 5.5.0
            if (securityToken is JsonWebToken  jsonWebToken)
            {
                var tid = jsonWebToken.GetPayloadValue<string>(ClaimConstants.Tid);
                if (tid != null)
                    return tid;
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// An implementation of IConfigurationRetriever geared towards Azure AD issuers metadata />
    /// </summary>
    public class IssuerConfigurationRetriever : IConfigurationRetriever<IssuerMetadata>
    {
        /// <summary>Retrieves a populated configuration given an address and an <see cref="T:Microsoft.IdentityModel.Protocols.IDocumentRetriever"/>.</summary>
        /// <param name="address">Address of the discovery document.</param>
        /// <param name="retriever">The <see cref="T:Microsoft.IdentityModel.Protocols.IDocumentRetriever"/> to use to read the discovery document.</param>
        /// <param name="cancel">A cancellation token that can be used by other objects or threads to receive notice of cancellation. <see cref="T:System.Threading.CancellationToken"/>.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="address"/> is null or empty.
        /// or
        /// retriever - No metadata document retriever is provided</exception>
        public async Task<IssuerMetadata> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException(nameof(address), $"Azure AD Issuer metadata address url is required");

            if (retriever == null)
                throw new ArgumentNullException(nameof(retriever), $"No metadata document retriever is provided");

            string doc = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<IssuerMetadata>(doc);
        }
    }

    /// <summary>
    /// Model class to hold information parsed from the Azure AD issuer endpoint
    /// </summary>
    public class IssuerMetadata
    {
        [JsonProperty(PropertyName = "tenant_discovery_endpoint")]
        public string TenantDiscoveryEndpoint { get; set; }

        [JsonProperty(PropertyName = "api-version")]
        public string ApiVersion { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public List<Metadata> Metadata { get; set; }
    }

    /// <summary>
    /// Model child class to hold alias information parsed from the Azure AD issuer endpoint.
    /// </summary>
    public class Metadata
    {
        [JsonProperty(PropertyName = "preferred_network")]
        public string PreferredNetwork { get; set; }

        [JsonProperty(PropertyName = "preferred_cache")]
        public string PreferredCache { get; set; }

        [JsonProperty(PropertyName = "aliases")]
        public List<string> Aliases { get; set; }
    }
}