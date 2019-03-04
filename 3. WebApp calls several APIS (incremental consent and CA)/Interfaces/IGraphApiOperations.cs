using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Interfaces
{
    public interface IGraphApiOperations
    {
        Task<dynamic> GetUserInformation(string accessToken);
        Task<string> GetPhotoAsBase64Async(string accessToken);
    }
}