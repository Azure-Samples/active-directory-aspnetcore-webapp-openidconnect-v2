using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph
{
    /// <summary>
    /// IMSGraph service shows how to call Microsoft Graph using the Graph SDK
    /// </summary>
    public interface IMSGraphService
    {
        Task<IList<DirectoryRole>> GetCurrentUserDirectoryRolesAsync(string accessToken);
        Task<UserGroupsAndDirectoryRoles> GetCurrentUserGroupsAndRolesAsync(string accessToken);
        Task<IList<Group>> GetCurrentUsersGroupsAsync(string accessToken);
        Task<User> GetMeAsync(string accessToken);
        Task<List<Group>> GetMyMemberOfGroupsAsync(string accessToken);
        Task<string> GetMyPhotoAsync(string accessToken);
        Task<List<User>> GetUsersAsync(string accessToken);
    }
}