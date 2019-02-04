using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            webOptions = webOptionValue.Value;
        }

        public async Task<dynamic> CallOnBehalfOfUserAsync(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(Constants.AuthorizationScheme,
                                              accessToken);
            var response = await httpClient.GetAsync(webOptions.GraphApiUrl);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic me = JsonConvert.DeserializeObject(content);

                return me;
            }

            throw new
                HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task<string> GetPhotoAsBase64Async(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(Constants.AuthorizationScheme,
                                              accessToken);

            var response = await httpClient.GetAsync("https://graph.microsoft.com/beta/me/photo/$value");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                byte[] photo = await response.Content.ReadAsByteArrayAsync();
                string photoBase64 = Convert.ToBase64String(photo);

                return photoBase64;
            }
            else
            {
                return null;
            }
        }
    }
}