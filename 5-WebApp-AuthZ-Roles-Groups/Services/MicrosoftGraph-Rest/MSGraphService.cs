using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Services.GraphOperations;

namespace WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph
{
    public class MSGraphService : IMSGraphService
    {
        private readonly WebOptions webOptions;

        // the Graph SDK's GraphServiceClient
        private GraphServiceClient graphServiceClient;

        public MSGraphService(IOptions<WebOptions> webOptionValue)
        {
            webOptions = webOptionValue.Value;
        }

        public async Task<User> GetMeAsync(string accessToken)
        {
            User currentUserObject;

            try
            {
                PrepareAuthenticatedClient(accessToken);
                currentUserObject = await graphServiceClient.Me.Request().GetAsync();
            }
            catch (ServiceException e)
            {
                Debug.WriteLine("We could not fetch details of the currently signed-in user: " + $"{e}");
                return null;
            }

            return currentUserObject;
        }

        public async Task<string> GetMyPhotoAsync(string accessToken)
        {
            PrepareAuthenticatedClient(accessToken);

            try
            {
                var photo = await graphServiceClient.Me.Photo.Content.Request().GetAsync();

                if (photo != null)
                {
                    using (photo)
                    {
                        // Get byte[] for display.
                        using (BinaryReader reader = new BinaryReader(photo))
                        {
                            byte[] data = reader.ReadBytes((int)photo.Length);
                            string photoBase64 = Convert.ToBase64String(data);

                            return photoBase64;
                        }
                    }
                }
            }
            catch (ServiceException sx)
            {
                if (sx.Error.Message == "The photo wasn't found.")
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return null;
        }

        // Get the groups that the current user is a direct member of.
        // This snippet requires an admin work account.
        public async Task<List<Group>> GetMyMemberOfGroupsAsync(string accessToken)
        {
            List<Group> groups = new List<Group>();

            // Get groups the current user is a direct member of.
            IUserMemberOfCollectionWithReferencesPage memberOfGroups = await graphServiceClient.Me.MemberOf.Request().GetAsync();
            if (memberOfGroups?.Count > 0)
            {
                foreach (var directoryObject in memberOfGroups)
                {
                    // We only want groups, so ignore DirectoryRole objects.
                    if (directoryObject is Group)
                    {
                        Group group = directoryObject as Group;
                        groups.Add(group);
                    }
                }
            }

            // If paginating
            while (memberOfGroups.NextPageRequest != null)
            {
                memberOfGroups = await memberOfGroups.NextPageRequest.GetAsync();

                if (memberOfGroups?.Count > 0)
                {
                    foreach (var directoryObject in memberOfGroups)
                    {
                        // We only want groups, so ignore DirectoryRole objects.
                        if (directoryObject is Group)
                        {
                            Group group = directoryObject as Group;
                            groups.Add(group);
                        }
                    }
                }
            }

            return groups;
        }

        public async Task<List<User>> GetUsersAsync(string accessToken)
        {
            List<User> allUsers = new List<User>();

            try
            {
                PrepareAuthenticatedClient(accessToken);
                IGraphServiceUsersCollectionPage users = await graphServiceClient.Users.Request().GetAsync();

                // When paginating
                //while(users.NextPageRequest != null)
                //{
                //    users = await users.NextPageRequest.GetAsync();
                //}

                if (users?.CurrentPage.Count > 0)
                {
                    foreach (User user in users)
                    {
                        allUsers.Add(user);
                    }
                }
            }
            catch (ServiceException e)
            {
                Debug.WriteLine("We could not retrieve the user's list: " + $"{e}");
                return null;
            }

            return allUsers;
        }

        private void PrepareAuthenticatedClient(string accessToken)
        {
            if (graphServiceClient == null)
            {
                // Create Microsoft Graph client.
                // graphServiceClient.BaseUrl = webOptions.GraphApiUrl;

                try
                {
                    graphServiceClient = new GraphServiceClient(webOptions.GraphApiUrl,
                                                                         new DelegateAuthenticationProvider(
                                                                             async (requestMessage) =>
                                                                             {
                                                                                 requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                                                                             }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not create a graph client {ex}");
                }
            }
        }
    }
}