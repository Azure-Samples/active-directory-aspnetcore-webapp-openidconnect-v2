using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Interfaces;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class GraphApiOperationService : IGraphApiOperations
    {
        private readonly HttpClient httpClient;
        private readonly WebOptions webOptions;

        public GraphApiOperationService(HttpClient httpClient, IOptions<WebOptions> webOptionValue)
        {
            this.httpClient = httpClient;
            webOptions      = webOptionValue.Value;
        }

        public async Task<dynamic> CallOnBehalfOfUserAsync(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(Constants.AuthenticationHeaderValue,
                                              accessToken);
            var response = await httpClient.GetAsync(webOptions.GraphApiUrl);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var     content = await response.Content.ReadAsStringAsync();
                dynamic me      = JsonConvert.DeserializeObject(content);
                return me;
            }

            throw new
                HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }
    }
}