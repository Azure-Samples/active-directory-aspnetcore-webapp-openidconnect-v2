using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.DAL;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.BLL
{
    public class TodoItemService : ITodoItemService
    {
        private readonly SampleDbContext sampleDbContext;
        public TodoItemService(SampleDbContext sampleDbContext)
        {
            this.sampleDbContext = sampleDbContext;
        }

        public async Task<TodoItem> Create(string text, ClaimsPrincipal loggedUser)
        {
            TodoItem todoItem = new TodoItem
            { 
                Text = text,
                UserName = loggedUser.Identity.Name,
                AssignedTo = loggedUser.GetObjectId(),
                TenantId = loggedUser.GetTenantId()
            };

            sampleDbContext.TodoItems.Add(todoItem);
            await sampleDbContext.SaveChangesAsync();

            return todoItem;
        }

        public async Task Delete(int id, ClaimsPrincipal loggedUser)
        {
            var todoItem = await Get(id, loggedUser);
            if (todoItem != null)
            {
                sampleDbContext.TodoItems.Remove(todoItem);
                await sampleDbContext.SaveChangesAsync();
            }
        }

        public async Task<TodoItem> Get(int id, ClaimsPrincipal loggedUser)
        {
            var tenantId = loggedUser.GetTenantId();
            var userIdentifier = loggedUser.GetObjectId();

            return await sampleDbContext.TodoItems.SingleOrDefaultAsync(x => 
                x.Id == id 
                && x.TenantId == tenantId 
                && x.AssignedTo == userIdentifier);
        }

        public async Task<IEnumerable<TodoItem>> List(bool showAll, ClaimsPrincipal loggedUser)
        {
            var tenantId = loggedUser.GetTenantId();
            var userIdentifier = loggedUser.GetObjectId();

            var filteredResult = sampleDbContext.TodoItems.Where(x => x.TenantId == tenantId);

            if (showAll)
                return await filteredResult.ToListAsync();
            else
                return await filteredResult.Where(x => x.AssignedTo == userIdentifier).ToListAsync();
        }
    }
}
