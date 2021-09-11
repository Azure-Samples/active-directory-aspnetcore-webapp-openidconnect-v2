using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using WebApp_OpenIDConnect_DotNet.Services.GraphOperations;

namespace WebApp_OpenIDConnect_DotNet.Services.Arm
{
    public class ArmApiOperationServiceWithImplicitAuth : IArmOperationsWithImplicitAuth
    {
        private readonly HttpClient httpClient;

        public ArmApiOperationServiceWithImplicitAuth(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Enumerates the list of Tenant IDs. Token for the user or app needs to be configured on the HTTP client.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> EnumerateTenantsIds()
        {
            var httpResult = await httpClient.GetAsync(ArmListTenantUrl);
            string json = await httpResult.Content.ReadAsStringAsync();
            ArmResult armTenants = JsonConvert.DeserializeObject<ArmResult>(json);
            return armTenants.value.Select(t => t.tenantId);
        }

        protected string ArmListTenantUrl { get; } = "https://management.azure.com/tenants?api-version=2016-06-01";
    }
}
