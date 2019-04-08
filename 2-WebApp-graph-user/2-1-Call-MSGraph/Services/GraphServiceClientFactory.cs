using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class GraphServiceClientFactory
    {
        public static async Task<GraphServiceClient> GetAuthenticatedGraphClient(Func<Task<string>> acquireAccessToken)
        {
            // Fetch the access token
            string accessToken = await acquireAccessToken.Invoke();

            return new GraphServiceClient(new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                            Infrastructure.Constants.BearerAuthorizationScheme, accessToken);
                    }));
        }
    }
}
