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
using Microsoft.Identity.Web.Resource;
using System.Collections.Generic;
using System.Linq;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [RequiredScope(scopeRequiredByAPI)]
    public class TodoListController : Controller
    {
        const string scopeRequiredByAPI = "access_as_user" ;
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        private readonly IHttpContextAccessor _contextAccessor;
     
        public TodoListController(IHttpContextAccessor contextAccessor)
        {
            this._contextAccessor = contextAccessor;
            string owner = this._contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;

            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new Todo() { Id = 1, Owner = owner, Title = "Pick up groceries" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = owner, Title = "Finish invoice report" });
            }
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<Todo> Get()
        {
            string owner = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            return TodoStore.Values.Where(x => x.Owner == owner);
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        public Todo Get(int id)
        {
            return TodoStore.Values.FirstOrDefault(t => t.Id == id);
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            TodoStore.Remove(id);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            string owner = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            int id = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;

            Todo todonew = new Todo() { Id = id, Owner = owner, Title = todo.Title };
            TodoStore.Add(id, todonew);

            return Ok(todo);
        }

        // PATCH api/values
        [HttpPatch("{id}")]
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