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

        /// <summary>
        /// Lists only items from the user's tenant
        /// </summary>
        /// <param name="showAll"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TodoItem>> List(bool showAll, ClaimsPrincipal user)
        {
            var tenantId = user.GetTenantId();
            var userIdentifier = user.GetObjectId();

            var filteredResult = sampleDbContext.TodoItems.Where(x => x.TenantId == tenantId);

            if (showAll)
                return await filteredResult.ToListAsync();
            else
                return await filteredResult.Where(x => x.AssignedTo == userIdentifier).ToListAsync();
        }

        public async Task<TodoItem> Get(int id, ClaimsPrincipal user)
        {
            var tenantId = user.GetTenantId();
            var userIdentifier = user.GetObjectId();

            return await sampleDbContext.TodoItems.SingleOrDefaultAsync(x =>
                x.Id == id
                && x.TenantId == tenantId
                && x.AssignedTo == userIdentifier);
        }

        public async Task<TodoItem> Create(string text, ClaimsPrincipal user)
        {
            //TodoItem table has the TenantId column so we can separate data from each different tenant, preserving its confidentiality
            TodoItem todoItem = new TodoItem
            {
                Text = text,
                UserName = user.Identity.Name,
                AssignedTo = user.GetObjectId(),
                TenantId = user.GetTenantId()
            };

            sampleDbContext.TodoItems.Add(todoItem);
            await sampleDbContext.SaveChangesAsync();

            return todoItem;
        }

        public async Task<TodoItem> Edit(TodoItem todoItem, ClaimsPrincipal user)
        {
            //Validate item ownership
            if (!IsAuthorizedToModify(todoItem.Id, user))
                throw new InvalidOperationException();

            sampleDbContext.TodoItems.Update(todoItem);
            await sampleDbContext.SaveChangesAsync();

            return todoItem;
        }

        public async Task Delete(int id, ClaimsPrincipal user)
        {
            //Validate item ownership
            if (!IsAuthorizedToModify(id, user))
                throw new InvalidOperationException();

            var todoItem = await Get(id, user);
            if (todoItem != null)
            {
                sampleDbContext.TodoItems.Remove(todoItem);
                await sampleDbContext.SaveChangesAsync();
            }
        }
        
        private bool IsAuthorizedToModify(int itemId, ClaimsPrincipal user)
        {
            var tenantId = user.GetTenantId();
            var userIdentifier = user.GetObjectId();

            // The user can only modify their own items
            return sampleDbContext.TodoItems
                .AsNoTracking()
                .Where(x => x.Id == itemId
                    && x.TenantId == tenantId
                    && x.AssignedTo == userIdentifier).Count() == 1;
        }
    }
}
