using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.BLL
{
    public interface IMSGraphService
    {
        Task<IEnumerable<User>> GetUsersAsync(string accessToken);
    }
}
