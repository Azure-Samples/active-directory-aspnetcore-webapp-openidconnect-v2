// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                TodoStore.Add(1, new Todo() { Id = 1, Owner = $"{_contextAccessor.HttpContext.User.Identity.Name}", Title = "Pick up groceries" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = $"{_contextAccessor.HttpContext.User.Identity.Name}", Title = "Finish invoice report" });
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
            if (IsInScopes(new string[] { "ToDoList.Read", "ToDoList.ReadWrite" }))
            {
                return TodoStore.Values.Where(x => x.Owner == User.Identity.Name);
            }
            else if (IsInPermissions(new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }))
            {
                return TodoStore.Values;
            }

            throw new ApplicationException("It's impossible for you to be in this state in a normal situation.");
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

            if (IsInScopes(new string[] { "ToDoList.Read", "ToDoList.ReadWrite" }))
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id && t.Owner == User.Identity.Name);
            }
            else if (IsInPermissions(new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }))
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id);
            }

            throw new ApplicationException("It's impossible for you to be in this state. STINKY HACKER");
        }

        [HttpDelete("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { "ToDoList.ReadWrite" },
            AcceptedAppPermission = new string[] { "ToDoList.ReadWrite.All" })]
        public void Delete(int id)
        {

            if (
                (

                IsInScopes(new string[] { "ToDoList.ReadWrite" }) && TodoStore.Values.Any(x => x.Id == id && x.Owner == User.Identity.Name))

                ||

                IsInPermissions(new string[] { "ToDoList.ReadWrite.All" })
                )
            {
                TodoStore.Remove(id);
            }

            else
            {
                throw new ApplicationException("No idea how you've got here");
            }
        }

        // POST api/values
        [HttpPost]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { "ToDoList.ReadWrite" },
            AcceptedAppPermission = new string[] { "ToDoList.ReadWrite.All" })]
        public IActionResult Post([FromBody] Todo todo)
        {
            var owner = HttpContext.User.Identity.Name;

            if (IsInPermissions(new string[] { "ToDoList.ReadWrite.All" }))
            {
                //with such a permission any owner name is accepted from UI
                owner = todo.Owner;
            }
            else
            {
                throw new ApplicationException("Somehow you've sneaked in. Impossible... Just a bug in Matrix...");
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
                IsInScopes(new string[] { "ToDoList.ReadWrite" }) 
                && TodoStore.Values.Any(x => x.Id == id && x.Owner == User.Identity.Name)
                && todo.Owner == User.Identity.Name

                ||

                IsInPermissions(new string[] { "ToDoList.ReadWrite.All" })

                )
            {
                TodoStore.Remove(id);
                TodoStore.Add(id, todo);

                return Ok(todo);
            }

            throw new ApplicationException("You have insufficient permissions to run this patch operation.");

        }

        //check if the permission is inside claims
        private bool IsInPermissions(string[] permissionsNames)
        {
            return User.Claims.Where(c => c.Type.Equals(ClaimTypes.Role)).FirstOrDefault()
                .Value.Split(' ').Any(v => permissionsNames.Any(p => p.Equals(v)));
        }

        //check if the scope is inside claims
        private bool IsInScopes(string[] scopesNames)
        {
            return User.Claims.Where(c => c.Type.Equals(Constants.ScopeClaimType)).FirstOrDefault()
                .Value.Split(' ').Any(v => scopesNames.Any(s => s.Equals(v)));
        }
    }
}