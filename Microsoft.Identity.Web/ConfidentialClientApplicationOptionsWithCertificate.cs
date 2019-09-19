using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extension of ConfidentialClientApplicationOptionsWithClientCertificate with information
    /// about the Client certificate
    /// </summary>
    public class ConfidentialClientApplicationOptionsWithClientCertificate : ConfidentialClientApplicationOptions
    {
        /// <summary>
        /// Client certificate used to prove the identity of the Web App / Web API
        /// this certificate was created by the app registration or uploaded to AAD
        /// </summary>
        public X509Certificate2 ClientCertificate { get; set; }
    }
}
