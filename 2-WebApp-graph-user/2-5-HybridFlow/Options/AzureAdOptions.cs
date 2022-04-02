using System.Globalization;
using Microsoft.Identity.Web;

namespace WebApp_OpenIDConnect_DotNet.Options
{
    /// <summary>
    /// Metadata designed to match application configurations for applications that call APIs.
    ///
    /// https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-app-configuration?tabs=aspnetcore
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// instance of Azure AD, for example public Azure or a Sovereign cloud (Azure China, Germany, US government, etc ...)
        /// </summary>
        public string Instance { get; set; } = "https://login.microsoftonline.com/";

        /// <summary>
        /// The Tenant is:
        /// - either the tenant ID of the Azure AD tenant in which this application is registered (a guid)
        /// or a domain name associated with the tenant
        /// - or 'organizations' (for a multi-tenant application)
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// The domain of the tenant
        /// </summary>
        public string? Domain { get; set; }


        /// <summary>
        /// Guid used by the application to uniquely identify itself to Azure AD
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// The ClientSecret is a credential used to authenticate the application to Azure AD.  Azure AD supports password and certificate credentials.
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Callback path added to redirect URI
        /// </summary>
        public string? CallbackPath { get; set; }

        /// <summary>
        /// Redirect URI to be used by WebApp (server)
        /// </summary>
        public string RedirectUri
        {
            get
            {
                var redirectUriBasePath = "https://localhost:7089";

                return $"{redirectUriBasePath}{CallbackPath}";
            }
        }

        /// <summary>
        /// URL of the authority
        /// </summary>
        public string Authority
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, Instance + "{0}{1}", Domain, "/v2.0");
            }
        }

        /// <summary>
        /// Name of a certificate in the user certificate store
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: the property above)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by this CertificateName property)
        /// <remarks> 
        public CertificateDescription? Certificate { get; set; }

    }
}