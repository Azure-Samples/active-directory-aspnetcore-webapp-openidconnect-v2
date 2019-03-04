using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Services.GraphOperations
{
    public interface IGraphApiOperations
    {
        Task<dynamic> GetUserInformation(string accessToken);
        Task<string> GetPhotoAsBase64Async(string accessToken);
    }
}