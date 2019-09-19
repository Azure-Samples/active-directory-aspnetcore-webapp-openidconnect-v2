using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Identity.Web
{
    public class ConfidentialClientApplicationOptionsWithClientCertificate : ConfidentialClientApplicationOptions
    {
        public X509Certificate2 ClientCertificate { get; set; }
    }
}
