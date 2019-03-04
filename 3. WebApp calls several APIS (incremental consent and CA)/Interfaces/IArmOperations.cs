using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Interfaces
{
    public interface IArmOperations
    {
        Task<IEnumerable<string>> EnumerateTenantsIdsAccessibleByUser(string accessToken);
    }
}