using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.BLL
{
    public interface ITodoItemService
    {
        Task<TodoItem> Create(string text, ClaimsPrincipal loggedUser);
        Task<IEnumerable<TodoItem>> List(bool showAll, ClaimsPrincipal loggedUser);
        Task<TodoItem> Get(int id, ClaimsPrincipal loggedUser);
        Task Delete(int id, ClaimsPrincipal loggedUser);
    }
}
