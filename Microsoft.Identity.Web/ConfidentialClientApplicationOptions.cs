using Microsoft.Identity.Client;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Configuration options for a confidential client application
    /// (Web app / Web API / daemon app). See https://aka.ms/msal-net/application-configuration
    /// </summary>
    public class ConfidentialClientApplicationOptions : ApplicationOptions
    {
        /// <summary>
        /// Client secret for the confidential client application. This secret (application password)
        /// is provided by the application registration portal, or provided to Azure AD during the
        /// application registration with PowerShell AzureAD, PowerShell AzureRM, or Azure CLI.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Location for private key certificate
        /// </summary>
        public string CertificateLocation { get; set; }

        /// <summary>
        /// Password for the private key certificate
        /// </summary>
        public string CertificatePassword { get; set; }
    }
}
