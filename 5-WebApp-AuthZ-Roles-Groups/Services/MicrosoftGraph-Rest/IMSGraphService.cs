using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph
{
    public interface IMSGraphService
    {
        Task<User> GetMeAsync(string accessToken);

        Task<List<Group>> GetMyMemberOfGroupsAsync(string accessToken);

        Task<string> GetMyPhotoAsync(string accessToken);

        Task<List<User>> GetUsersAsync(string accessToken);
    }
}