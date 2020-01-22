using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Services.GraphOperations;

namespace WebApp_OpenIDConnect_DotNet.Services.Arm
{
    public class ArmApiOperationService : IArmOperations
    {
        private readonly HttpClient httpClient;

        public ArmApiOperationService(HttpClient httpClient, IOptions<WebOptions> webOptionValue)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Enumerates the list of Tenant IDs accessible for a user. Gets a token for the user
        /// and calls the ARM API.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> EnumerateTenantsIdsAccessibleByUser(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var httpResult = await httpClient.GetAsync(ArmListTenantUrl);
            string json = await httpResult.Content.ReadAsStringAsync();
            ArmResult armTenants = JsonConvert.DeserializeObject<ArmResult>(json);
            return armTenants.value.Select(t => t.tenantId);
        }


        // Use Azure Resource manager to get the list of a tenant accessible by a user
        // https://docs.microsoft.com/en-us/rest/api/resources/tenants/list
        public static string ArmResource { get; } = "https://management.core.windows.net/";

        protected string ArmListTenantUrl { get; } = "https://management.azure.com/tenants?api-version=2016-06-01";
    }
}