using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet
{
    public class AzureAdB2COptions
    {
        public string ClientId { get; set; }
        public string SignUpSignInPolicy { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public string AzureAdB2CInstance { get; set; }
        public string Tenant { get; set; }
        public string MetadataAddress => $"{AzureAdB2CInstance}/{Tenant}/v2.0/.well-known/openid-configuration?p={SignUpSignInPolicy}";
    }
}
