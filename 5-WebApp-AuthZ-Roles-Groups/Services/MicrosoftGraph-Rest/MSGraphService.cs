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

        public async Task<IList<Group>> GetCurrentUsersGroupsAsync(string accessToken)
        {
            IUserMemberOfCollectionWithReferencesPage memberOfGroups = null;
            IList<Group> groups = new List<Group>();

            try
            {
                PrepareAuthenticatedClient(accessToken);

                memberOfGroups = await graphServiceClient.Me.MemberOf.Request().GetAsync();

                if (memberOfGroups != null)
                {
                    do
                    {
                        foreach (var directoryObject in memberOfGroups.CurrentPage)
                        {
                            if (directoryObject is Group)
                            {
                                Group group = directoryObject as Group;
                                // Trace.WriteLine("Got group: " + group.Id);
                                groups.Add(group as Group);
                            }
                        }
                        if (memberOfGroups.NextPageRequest != null)
                        {
                            memberOfGroups = await memberOfGroups.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            memberOfGroups = null;
                        }
                    } while (memberOfGroups != null);
                }

                return groups;
            }
            catch (ServiceException e)
            {
                Trace.Fail("We could not get user groups: " + e.Error.Message);
                return null;
            }
        }

        public async Task<IList<DirectoryRole>> GetCurrentUserDirectoryRolesAsync(string accessToken)
        {
            IUserMemberOfCollectionWithReferencesPage memberOfDirectoryRoles = null;
            IList<DirectoryRole> DirectoryRoles = new List<DirectoryRole>();

            try
            {
                PrepareAuthenticatedClient(accessToken);
                memberOfDirectoryRoles = await graphServiceClient.Me.MemberOf.Request().GetAsync();

                if (memberOfDirectoryRoles != null)
                {
                    do
                    {
                        foreach (var directoryObject in memberOfDirectoryRoles.CurrentPage)
                        {
                            if (directoryObject is DirectoryRole)
                            {
                                DirectoryRole DirectoryRole = directoryObject as DirectoryRole;
                                // Trace.WriteLine("Got DirectoryRole: " + DirectoryRole.Id);
                                DirectoryRoles.Add(DirectoryRole as DirectoryRole);
                            }
                        }
                        if (memberOfDirectoryRoles.NextPageRequest != null)
                        {
                            memberOfDirectoryRoles = await memberOfDirectoryRoles.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            memberOfDirectoryRoles = null;
                        }
                    } while (memberOfDirectoryRoles != null);
                }

                return DirectoryRoles;
            }
            catch (ServiceException e)
            {
                Trace.Fail("We could not get user DirectoryRoles: " + e.Error.Message);
                return null;
            }
        }

        /// <summary>
        /// A more efficient implementation that gets both group and role membership in one call
        /// </summary>
        /// <returns></returns>
        public async Task<UserGroupsAndDirectoryRoles> GetCurrentUserGroupsAndRolesAsync(string accessToken)
        {
            UserGroupsAndDirectoryRoles userGroupsAndDirectoryRoles = new UserGroupsAndDirectoryRoles();
            IUserMemberOfCollectionWithReferencesPage memberOfDirectoryRoles = null;

            try
            {
                PrepareAuthenticatedClient(accessToken);
                memberOfDirectoryRoles = await graphServiceClient.Me.MemberOf.Request().GetAsync();

                if (memberOfDirectoryRoles != null)
                {
                    do
                    {
                        foreach (var directoryObject in memberOfDirectoryRoles.CurrentPage)
                        {
                            if (directoryObject is Group)
                            {
                                Group group = directoryObject as Group;
                                // Trace.WriteLine($"Got group: {group.Id}- '{group.DisplayName}'");
                                userGroupsAndDirectoryRoles.Groups.Add(group);
                            }
                            else if (directoryObject is DirectoryRole)
                            {
                                DirectoryRole role = directoryObject as DirectoryRole;
                                // Trace.WriteLine($"Got DirectoryRole: {role.Id}- '{role.DisplayName}'");
                                userGroupsAndDirectoryRoles.DirectoryRoles.Add(role);
                            }
                        }
                        if (memberOfDirectoryRoles.NextPageRequest != null)
                        {
                            userGroupsAndDirectoryRoles.HasOverageClaim = true; //check if this matches 150 per token limit
                            memberOfDirectoryRoles = await memberOfDirectoryRoles.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            memberOfDirectoryRoles = null;
                        }
                    } while (memberOfDirectoryRoles != null);
                }

                return userGroupsAndDirectoryRoles;
            }
            catch (ServiceException e)
            {
                Trace.Fail("We could not get user groups and roles: " + e.Error.Message);
                return null;
            }
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

                try
                {
                    graphServiceClient = new GraphServiceClient(webOptions.GraphApiUrl,
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
}