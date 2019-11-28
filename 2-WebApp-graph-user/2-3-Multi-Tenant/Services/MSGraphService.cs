/*
 The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    /// <summary>Provides helper methods built over MS Graph SDK</summary>
    /// <seealso cref="WebApp_OpenIDConnect_DotNet.Services.IMSGraphService" />
    public class MSGraphService : IMSGraphService
    {
        // the Graph SDK's GraphServiceClient
        private GraphServiceClient graphServiceClient;
        private IConfiguration configuration;

        public MSGraphService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the users in a tenant.
        /// </summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>
        /// A list of users
        /// </returns>
        public async Task<IEnumerable<User>> GetUsersAsync(string accessToken)
        {
            IGraphServiceUsersCollectionPage users = null;

            try
            {
                PrepareAuthenticatedClient(accessToken);

                // Using Graph SDK to get users, filtering by active ones and returning just id and userPrincipalName field
                users = await graphServiceClient.Users.Request()
                    .Filter($"accountEnabled eq true")
                    .Select("id, userPrincipalName")
                    .GetAsync();

                if (users?.CurrentPage.Count > 0)
                {
                    return users;
                }
            }
            catch (ServiceException e)
            {
                Debug.WriteLine("We could not retrieve the user's list: " + $"{e}");
                return null;
            }

            return users;
        }

        /// <summary>
        /// Prepares the authenticated client.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        private void PrepareAuthenticatedClient(string accessToken)
        {
            try
            {
                /***
                //Microsoft Azure AD Graph API endpoint,
                'https://graph.microsoft.com'   Microsoft Graph global service
                'https://graph.microsoft.us' Microsoft Graph for US Government
                'https://graph.microsoft.de' Microsoft Graph Germany
                'https://microsoftgraph.chinacloudapi.cn' Microsoft Graph China
                 ***/

                string graphEndpoint = configuration.GetValue<string>("GraphAPI:Endpoint");
                graphServiceClient = new GraphServiceClient(graphEndpoint,
                                                                     new DelegateAuthenticationProvider(
                                                                         async (requestMessage) =>
                                                                         {
                                                                             await Task.Run(() =>
                                                                             {
                                                                                 requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                                                                             });
                                                                         }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not create a graph client {ex}");
            }
        }
    }
}