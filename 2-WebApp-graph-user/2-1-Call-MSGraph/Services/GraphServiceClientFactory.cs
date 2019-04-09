using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class GraphServiceClientFactory
    {
        public static GraphServiceClient GetAuthenticatedGraphClient(Func<Task<string>> acquireAccessToken, 
                                                                                 string baseUrl = null)
        {
  
            return new GraphServiceClient(baseUrl, new XX(acquireAccessToken));
        }
    }

    class XX : IAuthenticationProvider
    {
        public XX(Func<Task<string>> acquireAccessToken)
        {
            this.acquireAccessToken = acquireAccessToken;
        }

        private Func<Task<string>> acquireAccessToken;
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string accessToken = await acquireAccessToken.Invoke();

            // Append the access token to the request.
            request.Headers.Authorization = new AuthenticationHeaderValue(
                Infrastructure.Constants.BearerAuthorizationScheme, accessToken);

        }
    }
}
