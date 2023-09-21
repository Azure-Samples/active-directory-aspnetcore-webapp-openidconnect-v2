using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Threading.Tasks;
using TodoListService.Models;
using Microsoft.Identity.Abstractions;
using System.Net.Http;
using System.Collections.Generic;

namespace TodoListClient.Controllers
{
    [AuthorizeForScopes(ScopeKeySection = "TodoList:Scopes")]
    public class TodoListController : Controller
    {
        private IDownstreamApi _downstreamApi;

        public TodoListController(IDownstreamApi todoListService)
        {
            _downstreamApi = todoListService;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _downstreamApi.GetForUserAsync<IEnumerable<Todo>>("TodoList");
            return View(result);
        }

        // GET: TodoList/Details/5
        public async Task<ActionResult> Details(int id)
        {
            return View(await _downstreamApi.GetForUserAsync<Todo>(
                "TodoList", 
                options => options.RelativePath = $"api/todolist/{id}"));
        }

        // GET: TodoList/Create
        public ActionResult Create()
        {
            Todo todo = new Todo() { Owner = HttpContext.User.Identity.Name };
            return View(todo);
        }

        // POST: TodoList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Title,Owner")] Todo todo)
        {
            await _downstreamApi.PostForUserAsync("TodoList", todo);
            return RedirectToAction("Index");
        }

        // GET: TodoList/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            Todo todo = await _downstreamApi.GetForUserAsync<Todo>(
                 "TodoList",
                 options => options.RelativePath = $"api/todolist/{id}");

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: TodoList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            todo = await _downstreamApi.CallApiForUserAsync<Todo, Todo>(
                 "TodoList", todo,
                 options => { options.RelativePath = $"api/todolist/{id}"; options.HttpMethod = HttpMethod.Patch.ToString(); }) ;
            return RedirectToAction("Index");
        }

        // GET: TodoList/Delete/5
        public async Task<ActionResult> DeleteItem(int id)
        {
            Todo todo = await _downstreamApi.GetForUserAsync<Todo>(
                      "TodoList",
                      options => options.RelativePath = $"api/todolist/{id}");

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: TodoList/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteItem(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            await _downstreamApi.DeleteForUserAsync("TodoList", todo,
                options  => options.RelativePath = $"api/todolist/{id}");
            return RedirectToAction("Index");
        }
    }
}