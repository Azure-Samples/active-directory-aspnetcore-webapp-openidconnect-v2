/*
 The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoListController : ControllerBase
    {
        // In-memory TodoList
        private static readonly Dictionary<int, TodoItem> TodoStore = new Dictionary<int, TodoItem>();
        private readonly IHttpContextAccessor _contextAccessor;

        public TodoListController(IHttpContextAccessor contextAccessor)
        {
            this._contextAccessor = contextAccessor;

            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new TodoItem() { Id = 1, Owner = $"{this._contextAccessor.HttpContext.User.Identity.Name}", Title = "Pick up groceries" });
                TodoStore.Add(2, new TodoItem() { Id = 2, Owner = $"{this._contextAccessor.HttpContext.User.Identity.Name}", Title = "Finish invoice report" });
            }
        }

        // GET: api/todolist
        [HttpGet]
        public IActionResult Get()
        {
            string owner = User.Identity.Name;
            var items = TodoStore.Values.Where(x => x.Owner == owner).ToList();
            return Ok(items);
        }

        // GET: api/todolist/1
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            var item = TodoStore.Values.FirstOrDefault(t => t.Id == id);
            return Ok(item);
        }

        // POST api/todolist
        [HttpPost]
        public IActionResult Post([FromBody] TodoItem todo)
        {
            int id = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            TodoItem newTodo = new TodoItem() { Id = id, Owner = HttpContext.User.Identity.Name, Title = todo.Title };
            TodoStore.Add(id, newTodo);

            return Ok(todo);
        }

        // PATCH api/todolist
        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] TodoItem todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            string owner = User.Identity.Name;

            if (TodoStore.Values.FirstOrDefault(x => x.Id == id && x.Owner == owner) == null)
            {
                return NotFound();
            }

            TodoStore.Remove(id);
            TodoStore.Add(id, todo);

            return Ok(todo);
        }

        // DELETE api/todolist/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string owner = User.Identity.Name;
            if (TodoStore.Values.FirstOrDefault(x => x.Id == id && x.Owner == owner) == null)
            {
                return NotFound();
            }

            TodoStore.Remove(id);
            return Ok();
        }
    }
}
