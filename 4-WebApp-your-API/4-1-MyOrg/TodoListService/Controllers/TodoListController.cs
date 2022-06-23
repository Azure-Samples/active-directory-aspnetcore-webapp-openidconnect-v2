// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TodoListClient.Models;
using WebApp_OpenIDConnect_DotNet.Infrastructure;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        private readonly IHttpContextAccessor _contextAccessor;

        public TodoListController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;

            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new Todo() { Id = 1, Owner = $"{GetObjectIdClaim(_contextAccessor.HttpContext.User)}", Title = "Pick up groceries" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = $"{GetObjectIdClaim(_contextAccessor.HttpContext.User)}", Title = "Finish invoice report" });
                TodoStore.Add(3, new Todo() { Id = 3, Owner = "Other User", Title = "Rent a car" });
                TodoStore.Add(4, new Todo() { Id = 4, Owner = "Other User", Title = "Get vaccinated" });
            }
        }

        // GET: api/values
        [HttpGet]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { "ToDoList.Read", "ToDoList.ReadWrite" },
            AcceptedAppPermission = new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }
            )]
        public IEnumerable<Todo> Get()
        {
            if (HasDelegatedPermissions(new string[] { "ToDoList.Read", "ToDoList.ReadWrite" }))
            {
                return TodoStore.Values.Where(x => x.Owner == GetObjectIdClaim(User));
            }
            else if (HasApplicationPermissions(new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }))
            {
                return TodoStore.Values;
            }

            return null;
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { "ToDoList.Read", "ToDoList.ReadWrite" },
            AcceptedAppPermission = new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" })]
        public Todo Get(int id)
        {
            //if it only has delegated permissions
            //then it will be t.id==id && x.Owner == owner
            //if it has app permissions the it will return  t.id==id

            if (HasDelegatedPermissions(new string[] { "ToDoList.Read", "ToDoList.ReadWrite" }))
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id && t.Owner == GetObjectIdClaim(User));
            }
            else if (HasApplicationPermissions(new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }))
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id);
            }

            return null;
        }

        [HttpDelete("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { "ToDoList.ReadWrite" },
            AcceptedAppPermission = new string[] { "ToDoList.ReadWrite.All" })]
        public void Delete(int id)
        {
            if (
                (

                HasDelegatedPermissions(new string[] { "ToDoList.ReadWrite" }) && TodoStore.Values.Any(x => x.Id == id && x.Owner == GetObjectIdClaim(User)))

                ||

                HasApplicationPermissions(new string[] { "ToDoList.ReadWrite.All" })
                )
            {
                TodoStore.Remove(id);
            }
        }

        // POST api/values
        [HttpPost]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { "ToDoList.ReadWrite" },
            AcceptedAppPermission = new string[] { "ToDoList.ReadWrite.All" })]
        public IActionResult Post([FromBody] Todo todo)
        {
            var owner = GetObjectIdClaim(User);

            if (HasApplicationPermissions(new string[] { "ToDoList.ReadWrite.All" }))
            {
                //with such a permission any owner name is accepted from UI
                owner = todo.Owner;
            }

            int id = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            Todo todonew = new Todo() { Id = id, Owner = owner, Title = todo.Title };
            TodoStore.Add(id, todonew);

            return Ok(todo);
        }

        // PATCH api/values
        [HttpPatch("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { "ToDoList.ReadWrite" },
            AcceptedAppPermission = new string[] { "ToDoList.ReadWrite.All" })]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            if (id != todo.Id || !TodoStore.Values.Any(x => x.Id == id))
            {
                return NotFound();
            }

            if (
                HasDelegatedPermissions(new string[] { "ToDoList.ReadWrite" })
                && TodoStore.Values.Any(x => x.Id == id && x.Owner == GetObjectIdClaim(User))
                && todo.Owner == GetObjectIdClaim(User)

                ||

                HasApplicationPermissions(new string[] { "ToDoList.ReadWrite.All" })

                )
            {
                TodoStore.Remove(id);
                TodoStore.Add(id, todo);

                return Ok(todo);
            } 

            return BadRequest();
        }

        //Checks if the presented token has application permissions
        private bool HasApplicationPermissions(string[] permissionsNames)
        {
            var rolesClaim = User.Claims.Where(
              c => c.Type == ClaimConstants.Roles || c.Type == ClaimConstants.Role)
              .SelectMany(c => c.Value.Split(' '));

            var result = rolesClaim.Any(v => permissionsNames.Any(p => p.Equals(v)));

            return result;
        }

        //Checks if the presented token has delegated permissions
        private bool HasDelegatedPermissions(string[] scopesNames)
        {
            var result = (User.FindFirst(ClaimConstants.Scp) ?? User.FindFirst(ClaimConstants.Scope))?
                .Value.Split(' ').Any(v => scopesNames.Any(s => s.Equals(v)));

            return result ?? false;
        }

        private string GetObjectIdClaim(ClaimsPrincipal user)
        {
            return (user.FindFirst(ClaimConstants.Oid) ?? user.FindFirst(ClaimConstants.ObjectId))?.Value;
        }
    }
}