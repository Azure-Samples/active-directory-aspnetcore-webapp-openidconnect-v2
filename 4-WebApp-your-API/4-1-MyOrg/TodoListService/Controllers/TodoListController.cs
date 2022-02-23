// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Collections.Generic;
using System.Linq;
using TodoListService.Models;

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
            }
        }

        // GET: api/values
        [HttpGet]
        [RequiredScope(new string[] { "ToDoList.Read", "ToDoList.Write" })]
        public IEnumerable<Todo> Get()
        {
            string owner = User.Identity.Name;
            return TodoStore.Values.Where(x => x.Owner == owner);
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        [RequiredScope(new string[] { "ToDoList.Read", "ToDoList.Write" })]
        public Todo Get(int id)
        {
            return TodoStore.Values.FirstOrDefault(t => t.Id == id);
        }

        [HttpDelete("{id}")]
        [RequiredScope("ToDoList.Write")]
        public void Delete(int id)
        {
            TodoStore.Remove(id);
        }

        // POST api/values
        [HttpPost]
        [RequiredScope("ToDoList.Write")]
        public IActionResult Post([FromBody] Todo todo)
        {
            int id = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            Todo todonew = new Todo() { Id = id, Owner = HttpContext.User.Identity.Name, Title = todo.Title };
            TodoStore.Add(id, todonew);

            return Ok(todo);
        }

        // PATCH api/values
        [HttpPatch("{id}")]
        [RequiredScope("ToDoList.Write")]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            if (TodoStore.Values.FirstOrDefault(x => x.Id == id) == null)
            {
                return NotFound();
            }

            TodoStore.Remove(id);
            TodoStore.Add(id, todo);

            return Ok(todo);
        }
    }
}