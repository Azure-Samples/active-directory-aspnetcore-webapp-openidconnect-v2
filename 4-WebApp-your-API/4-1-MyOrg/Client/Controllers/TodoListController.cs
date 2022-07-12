using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Threading.Tasks;
using TodoListClient.Models;
using TodoListClient.Services;

namespace TodoListClient.Controllers
{
    public class TodoListController : Controller
    {
        private ITodoListService _todoListService;

        public TodoListController(ITodoListService todoListService)
        {
            _todoListService = todoListService;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _todoListService.GetAsync();
            return View(result);
        }

        public async Task<ActionResult> Details(int id)
        {
            return View(await _todoListService.GetAsync(id));
        }

        public ActionResult Create()
        {
            Todo todo = new Todo() { Owner = HttpContext.User.Identity.Name };
            return View(todo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Title,Owner")] Todo todo)
        {
            await _todoListService.AddAsync(todo);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int id)
        {
            Todo todo = await this._todoListService.GetAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            await _todoListService.EditAsync(todo);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int id)
        {
            Todo todo = await this._todoListService.GetAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            await _todoListService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}