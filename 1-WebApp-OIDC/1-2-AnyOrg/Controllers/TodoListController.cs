using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.BLL;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private readonly ITodoItemService _todoItemService;
        public TodoListController(ITodoItemService todoItemService)
        {
            _todoItemService = todoItemService;
        }

        public async Task<IActionResult> Index(bool showAllFilter)
        {
            @TempData["ShowAllFilter"] = showAllFilter;

            var items = await _todoItemService.List(showAllFilter, User);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new TodoItem()
            {
                AssignedTo = User.GetObjectId(),
                TenantId = User.GetTenantId(),
                UserName = User.Identity.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoItem model)
        {
            await _todoItemService.Create(model.Text, User);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            await _todoItemService.Delete(id, User);
            return RedirectToAction("Index");
        }
    }
}