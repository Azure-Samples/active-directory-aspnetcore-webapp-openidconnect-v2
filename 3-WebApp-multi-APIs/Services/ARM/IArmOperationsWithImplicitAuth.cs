using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Services.Arm
{
    public interface IArmOperationsWithImplicitAuth
    {
        Task<IEnumerable<string>> EnumerateTenantsIds();
    }
}
