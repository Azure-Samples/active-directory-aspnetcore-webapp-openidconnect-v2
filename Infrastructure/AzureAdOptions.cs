namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public class AzureAdOptions
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallBackPath { get; set; }
        public string ClientSecret { get; set; }
    }
}