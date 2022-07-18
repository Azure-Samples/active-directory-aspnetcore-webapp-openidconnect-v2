// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using TodoListClient.Models;


namespace TodoListService.Controllers
{
    /// <summary>
    /// This is an example of ToDo list controller that serves requests from client apps that sign-in users and as themselves (client credentials flow).
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        // This is needed to get access to the internal HttpContext.User, if available.
        private readonly IHttpContextAccessor _contextAccessor;

        private const string _todoListReadScope = "ToDoList.Read";
        private const string _todoListReadWriteScope = "ToDoList.ReadWrite";
        private const string _todoListReadAllPermission = "ToDoList.Read.All";
        private const string _todoListReadWriteAllPermission = "ToDoList.ReadWrite.All";

        /// <summary>
        /// We store the object id of the user derived from the presented Access token
        /// </summary>
        private string _currentLoggedUser = string.Empty;

        /// <summary>
        /// The API controller that manages an instance of ToDo list
        /// </summary>
        /// <param name="contextAccessor"></param>
        public TodoListController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;

            // We seek the details of the user represented by the access token presented to this API, can be empty
            /**
             * The 'oid' (object id) is the only claim that should be used to uniquely identify
             * a user in an Azure AD tenant. The token might have one or more of the following claim,
             * that might seem like a unique identifier, but is not and should not be used as such,
             * especially for systems which act as system of record (SOR):
             *
             * - upn (user principal name): might be unique amongst the active set of users in a tenant but
             * tend to get reassigned to new employees as employees leave the organization and
             * others take their place or might change to reflect a personal change like marriage.
             *
             * - email: might be unique amongst the active set of users in a tenant but tend to get
             * reassigned to new employees as employees leave the organization and others take their place.
             **/

            _currentLoggedUser = _contextAccessor?.HttpContext?.User?.GetObjectId();

            if (!string.IsNullOrWhiteSpace(_currentLoggedUser))
            {
                // Pre-populate with sample data
                if (TodoStore.Count == 0 && !string.IsNullOrEmpty(_currentLoggedUser))
                {
                    TodoStore.Add(1, new Todo() { Id = 1, Owner = $"{_currentLoggedUser}", Title = "Pick up groceries" });
                    TodoStore.Add(2, new Todo() { Id = 2, Owner = $"{_currentLoggedUser}", Title = "Finish invoice report" });
                    TodoStore.Add(3, new Todo() { Id = 3, Owner = "Fake id of another User", Title = "Rent a car" });
                    TodoStore.Add(4, new Todo() { Id = 4, Owner = "made up id of another ", Title = "Get vaccinated" });
                }
            }
        }

        /// <summary>
        /// Indicates of the AT presented was for a app or not.
        /// </summary>
        /// <returns></returns>
        private bool IsAppOnlyToken()
        {
            // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
            //
            // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims
            return HttpContext.User.Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
        }

        // GET: api/values
        /// <summary>
        /// Returns todo list items in a list
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }
            )]
        public IEnumerable<Todo> Get()
        {
            if (!IsAppOnlyToken())
            {
                // this is a request for all ToDo list items of a certain user.
                return TodoStore.Values.Where(x => x.Owner == _currentLoggedUser);
            }
            else
            {
                // Its an app calling with app permissions, so return all items across all users
                return TodoStore.Values;
            }
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission })]
        public Todo Get(int id)
        {
            //if it only has delegated permissions
            //then it will be t.id==id && x.Owner == owner
            //if it has app permissions the it will return  t.id==id

            if (!IsAppOnlyToken())
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id && t.Owner == _currentLoggedUser);
            }
            else
            {
                return TodoStore.Values.FirstOrDefault(t => t.Id == id);
            }
        }

        // DELETE: TodoList/Delete/5
        [HttpDelete("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public void Delete(int id)
        {
            if (!IsAppOnlyToken())
            {
                // only delete if the ToDo list item belonged to this user
                if (TodoStore.Values.Any(x => x.Id == id && x.Owner == _currentLoggedUser))
                {
                    TodoStore.Remove(id);
                }
            }
            else
            {
                TodoStore.Remove(id);
            }
        }

        // POST: TodoList/Create
        [HttpPost]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public IActionResult Post([FromBody] Todo todo)
        {
            if (IsAppOnlyToken())
            {
                if (string.IsNullOrEmpty(todo.Owner))
                {
                    var msg = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "The owner's objectid was not provided in the ToDo list item payload"
                    };

                    return BadRequest(msg);
                }
            }
            else
            {
                // The signed-in user becomes the owner
                todo.Owner = _currentLoggedUser;
            }

            int nextid = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;

            todo.Id = nextid;
            TodoStore.Add(nextid, todo);
            return Created($"/todo/{nextid}", todo);
        }

        [HttpPatch("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            Todo existingToDo = TodoStore.Values.FirstOrDefault(x => x.Id == id);

            if (id != todo.Id || existingToDo == null)
            {
                return NotFound();
            }

            if (!IsAppOnlyToken())
            {
                // a user can only modify their own ToDos
                if (existingToDo.Owner != _currentLoggedUser)
                {
                    return Unauthorized();
                }

                // Overwrite ownership, just in case
                todo.Owner = _currentLoggedUser;
            }

            TodoStore.Remove(id);
            TodoStore.Add(id, todo);
            return Ok(todo);
        }
    }
}