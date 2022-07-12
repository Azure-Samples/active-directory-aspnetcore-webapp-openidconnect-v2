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

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        private readonly IHttpContextAccessor _contextAccessor;

        private const string _todoListReadScope = "ToDoList.Read";
        private const string _todoListReadWriteScope = "ToDoList.ReadWrite";
        private const string _todoListReadAllPermission = "ToDoList.Read.All";
        private const string _todoListReadWriteAllPermission = "ToDoList.ReadWrite.All";

        private string _currentLoggedUser = string.Empty;

        public TodoListController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            _currentLoggedUser = _contextAccessor.HttpContext.User.GetObjectId();

            // Pre-populate with sample data
            if (TodoStore.Count == 0 && !string.IsNullOrEmpty(_currentLoggedUser))
            {
                TodoStore.Add(1, new Todo() { Id = 1, Owner = $"{_currentLoggedUser}", Title = "Pick up groceries" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = $"{_currentLoggedUser}", Title = "Finish invoice report" });
                TodoStore.Add(3, new Todo() { Id = 3, Owner = "Other User", Title = "Rent a car" });
                TodoStore.Add(4, new Todo() { Id = 4, Owner = "Other User", Title = "Get vaccinated" });
            }
        }

        [HttpGet]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }
            )]
        public IEnumerable<Todo> Get()
        {
            if (HasDelegatedPermissions(new string[] { _todoListReadScope, _todoListReadWriteScope }))
            {
                return TodoStore.Values.Where(x => x.Owner == _currentLoggedUser);
            }
            else if (HasApplicationPermissions(new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }))
            {
                return TodoStore.Values;
            }

            return null;
        }

        [HttpGet("{id}", Name = "Get")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission })]
        public Todo Get(int id)
        {
            //if it only has delegated permissions
            //then it will be t.id==id && x.Owner == owner
            //if it has app permissions the it will return  t.id==id

            if (HasDelegatedPermissions(new string[] { _todoListReadScope, _todoListReadWriteScope }))
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id && t.Owner == _currentLoggedUser);
            }
            else if (HasApplicationPermissions(new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }))
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id);
            }

            return null;
        }

        [HttpDelete("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public void Delete(int id)
        {
            if (
                (

                HasDelegatedPermissions(new string[] { _todoListReadWriteScope }) && TodoStore.Values.Any(x => x.Id == id && x.Owner == _currentLoggedUser))

                ||

                HasApplicationPermissions(new string[] { _todoListReadWriteAllPermission })
                )
            {
                TodoStore.Remove(id);
            }
        }

        [HttpPost]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public IActionResult Post([FromBody] Todo todo)
        {
            var ownerInEffect = _currentLoggedUser;

            if (HasApplicationPermissions(new string[] { _todoListReadWriteAllPermission }))
            {
                //with such a permission any owner name is accepted from UI
                ownerInEffect = todo.Owner;
            }

            int id = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            Todo todonew = new Todo() { Id = id, Owner = ownerInEffect, Title = todo.Title };
            TodoStore.Add(id, todonew);

            return Ok(todo);
        }

        [HttpPatch("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            if (id != todo.Id || !TodoStore.Values.Any(x => x.Id == id))
            {
                return NotFound();
            }

            if (
                HasDelegatedPermissions(new string[] { _todoListReadWriteScope })
                && TodoStore.Values.Any(x => x.Id == id && x.Owner == _currentLoggedUser)
                && todo.Owner == _currentLoggedUser

                ||

                HasApplicationPermissions(new string[] { _todoListReadWriteAllPermission })

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

        /// <summary>
        /// Redundant but left as an alternative to _contextAccessor.HttpContext.User.GetObjectId()
        /// </summary>
        /// <param name="user"> Claim Principal</param>
        /// <returns></returns>
        private string GetObjectIdClaim(ClaimsPrincipal user)
        {
            return (user.FindFirst(ClaimConstants.Oid) ?? user.FindFirst(ClaimConstants.ObjectId))?.Value;
        }
    }
}