// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Web.InstanceDiscovery;

namespace Microsoft.Identity.Web.Resource
{
    /// <summary>
    /// Generic class that validates token issuer from the provided Azure AD authority. Use the <see cref="AadIssuerValidatorFactory"/> to create instances of this class.
    /// </summary>
    public class AadIssuerValidator
    {
        private const string AzureADIssuerMetadataUrl = "https://login.microsoftonline.com/common/discovery/instance?authorization_endpoint=https://login.microsoftonline.com/common/oauth2/v2.0/authorize&api-version=1.1";
        private const string FallbackAuthority = "https://login.microsoftonline.com/";

        // TODO: separate AadIssuerValidator creation logic from the validation logic in order to unit test it
        private static readonly IDictionary<string, AadIssuerValidator> s_issuerValidators = new ConcurrentDictionary<string, AadIssuerValidator>();

        private static readonly ConfigurationManager<IssuerMetadata> s_configManager = new ConfigurationManager<IssuerMetadata>(AzureADIssuerMetadataUrl, new IssuerConfigurationRetriever());

        /// <summary>
        /// A list of all Issuers across the various Azure AD instances
        /// </summary>
        private readonly ISet<string> _issuerAliases;

        internal /* internal for test */ AadIssuerValidator(IEnumerable<string> aliases)
        {
            _issuerAliases = new HashSet<string>(aliases, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a <see cref="AadIssuerValidator"/> for an authority.
        /// </summary>
        /// <param name="aadAuthority">The authority to create the validator for, e.g. https://login.microsoftonline.com/ </param>
        /// <returns>A <see cref="AadIssuerValidator"/> for the aadAuthority.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="aadAuthority"/> is null or empty.</exception>
        public static AadIssuerValidator GetIssuerValidator(string aadAuthority)
        {
            if (string.IsNullOrEmpty(aadAuthority))
                throw new ArgumentNullException(nameof(aadAuthority));

            if (s_issuerValidators.TryGetValue(aadAuthority, out AadIssuerValidator aadIssuerValidator))
            {
                return aadIssuerValidator;
            }
            else
            {
                // In the constructor, we hit the Azure AD issuer metadata endpoint and cache the aliases. The data is cached for 24 hrs.
                var issuerMetadata = s_configManager.GetConfigurationAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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
                string authority = authorityHost ?? new Uri(FallbackAuthority).Host;
                var aliases = issuerMetadata.Metadata
                    .Where(m => m.Aliases.Any(a => string.Equals(a, authority, StringComparison.OrdinalIgnoreCase)))
                    .SelectMany(m => m.Aliases)
                    .Append(authority) // For b2c scenarios, the alias will be the authorityHost itself
                    .Distinct();
                s_issuerValidators[authority] = new AadIssuerValidator(aliases);
                return s_issuerValidators[authority];
            }
        }

        /// <summary>
        /// Validate the issuer for multi-tenant applications of various audience (Work and School account, or Work and School accounts +
        /// Personal accounts)
        /// </summary>
        /// <param name="actualIssuer">Issuer to validate (will be tenanted)</param>
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
        public string Validate(string actualIssuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (String.IsNullOrEmpty(actualIssuer))
                throw new ArgumentNullException(nameof(actualIssuer));

            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            if (validationParameters == null)
                throw new ArgumentNullException(nameof(validationParameters));

            string tenantId = GetTenantIdFromToken(securityToken);
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new SecurityTokenInvalidIssuerException("Neither `tid` nor `tenantId` claim is present in the token obtained from Microsoft identity platform.");

            if (validationParameters.ValidIssuers != null)
                foreach (var validIssuerTemplate in validationParameters.ValidIssuers)
                    if (IsValidIssuer(validIssuerTemplate, tenantId, actualIssuer))
                        return actualIssuer;

            if (IsValidIssuer(validationParameters.ValidIssuer, tenantId, actualIssuer))
                return actualIssuer;

            // If a valid issuer is not found, throw
            // brentsch - todo, create a list of all the possible valid issuers in TokenValidationParameters
            throw new SecurityTokenInvalidIssuerException($"Issuer: '{actualIssuer}', does not match any of the valid issuers provided for this application.");
        }

        private bool IsValidIssuer(string validIssuerTemplate, string tenantId, string actualIssuer)
        {
            if (string.IsNullOrEmpty(validIssuerTemplate))
                return false;

            try
            {
                var issuerFromTemplateUri = new Uri(validIssuerTemplate.Replace("{tenantid}", tenantId));
                var actualIssuerUri = new Uri(actualIssuer);

                // Template authority is in the aliases
                return _issuerAliases.Contains(issuerFromTemplateUri.Authority) &&
                       // "iss" authority is in the aliases
                       _issuerAliases.Contains(actualIssuerUri.Authority) &&
                      // Template authority ends in the tenantId
                      IsValidTidInLocalPath(tenantId, issuerFromTemplateUri) &&
                      // "iss" ends in the tenantId
                      IsValidTidInLocalPath(tenantId, actualIssuerUri);
            }
            catch
            {
                // if something faults, ignore
            }

            return false;
        }

        private static bool IsValidTidInLocalPath(string tenantId, Uri uri)
        {
            string trimmedLocalPath = uri.LocalPath.Trim('/');
            return trimmedLocalPath == tenantId || trimmedLocalPath == $"{tenantId}/v2.0";
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

                // Since B2C doesn't have TID as default, try to get it from iss claims
                if (jwtSecurityToken.Claims.Any(c => c.Type == ClaimConstants.Acr || c.Type == ClaimConstants.Tfp))
                    return GetTenantIdFromIss(jwtSecurityToken.Issuer);
            }

            // brentsch - todo, TryGetPayloadValue is available in 5.5.0
            if (securityToken is JsonWebToken jsonWebToken)
            {
                var tid = jsonWebToken.GetPayloadValue<string>(ClaimConstants.Tid);
                if (tid != null)
                    return tid;

                // Since B2C doesnt have TID as default, try to get it from iss claims
                if (jsonWebToken.Claims.Any(c => c.Type == ClaimConstants.Acr || c.Type == ClaimConstants.Tfp))
                    return GetTenantIdFromIss(jsonWebToken.Issuer);
            }

            return string.Empty;
        }

        // The AAD iss claims contains the tenantId in its value. The uri is {domain}/{tid}/v2.0
        private static string GetTenantIdFromIss(string iss)
        {
            if (string.IsNullOrEmpty(iss))
                return string.Empty;

            var uri = new Uri(iss);

            if (uri.Segments.Length > 1)
            {
                return uri.Segments[1].TrimEnd('/');
            }

            return string.Empty;
        }
    }
}