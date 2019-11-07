using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.BLL
{
    public class MSGraphService : IMSGraphService
    {
        // the Graph SDK's GraphServiceClient
        private GraphServiceClient graphServiceClient;

        /// <summary>
        /// Gets the users in a tenant.
        /// </summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>
        /// A list of users
        /// </returns>
        public async Task<IEnumerable<User>> GetUsersAsync(string accessToken)
        {
            var usersDropDown = new List<SelectListItem>();
            
            try
            {
                PrepareAuthenticatedClient(accessToken);
                IGraphServiceUsersCollectionPage users = await graphServiceClient.Users.Request()
                    .Filter($"accountEnabled eq true")
                    .Select("id, userPrincipalName")
                    .GetAsync();
                
                if (users?.CurrentPage.Count > 0)
                {
                    usersDropDown = users.Select(u => new SelectListItem 
                                    { 
                                        Text = u.UserPrincipalName,
                                        Value = u.Id
                                    }).ToList();
                }
            }
            catch (ServiceException e)
            {
                Debug.WriteLine("We could not retrieve the user's list: " + $"{e}");
                return null;
            }
            
            return usersDropDown;
        }

        /// <summary>
        /// Prepares the authenticated client.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        private void PrepareAuthenticatedClient(string accessToken)
        {
            try
            {
                graphServiceClient = new GraphServiceClient("https://graph.microsoft.com/beta",
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
