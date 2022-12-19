using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoListClient.Models;
using ToDoListClient.Services;
using ToDoListClient.Utils;

namespace ToDoListClient.Controllers
{
    [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListServiceScope")]
    public class ToDoListController : Controller
    {
        private IToDoListService _todoListService;

        public ToDoListController(IToDoListService todoListService)
        {
            _todoListService = todoListService;
        }

        // GET: TodoList
        public async Task<ActionResult> Index()
        {
            TempData["SignedInUser"] = User.GetDisplayName();
            return View(await _todoListService.GetAsync());
        }

        // GET: TodoList/Create
        public async Task<IActionResult> Create()
        {
            ToDoItem todo = new ToDoItem();
            var signedInUser = HttpContext.User.GetDisplayName();
            try
            {
                List<string> result = (await _todoListService.GetAllGraphUsersAsync()).ToList();

                //move signed in user to top of the list so it will be selected on Create ToDo item page
                result.Remove(signedInUser);
                result.Insert(0, signedInUser);

                TempData["UsersDropDown"] = result
                .Select(u => new SelectListItem
                {
                    Text = u
                }).ToList();
                TempData["TenantId"] = HttpContext.User.GetTenantId();
                TempData["AssignedBy"] = signedInUser;
                return View(todo);
            }
            catch (WebApiMsalUiRequiredException ex)
            {
                return Redirect(ex.Message);
            }
        }

        // POST: TodoList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Title,AssignedTo,AssignedBy,TenantId")] ToDoItem todo)
        {
            await _todoListService.AddAsync(todo);
            return RedirectToAction("Index");
        }

        // GET: TodoList/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            ToDoItem todo = await this._todoListService.GetAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: TodoList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Title,AssignedTo,AssignedBy,TenantId")] ToDoItem todo)
        {
            await _todoListService.EditAsync(todo);
            return RedirectToAction("Index");
        }

        // GET: TodoList/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            ToDoItem todo = await this._todoListService.GetAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: TodoList/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, [Bind("Id,Title,AssignedTo")] ToDoItem todo)
        {
            await _todoListService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}