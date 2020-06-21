using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using ToDoListClient.Models;
using ToDoListClient.Services;
using ToDoListClient.Utils;

namespace ToDoListClient.Controllers
{
    [Authorize]
    public class ToDoListController : Controller
    {
        private IToDoListService _todoListService;
        private readonly string _TodoListScope = string.Empty;
        private readonly string _ClientId = string.Empty;
        private readonly string _RedirectUri = string.Empty;

        public ToDoListController(IToDoListService todoListService, IConfiguration configuration)
        {
            _todoListService = todoListService;
            _TodoListScope = configuration["TodoList:TodoListScope"];
            _ClientId = configuration["AzureAd:ClientId"];
            _RedirectUri = configuration["RedirectUri"];
        }

        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        // GET: TodoList
        public async Task<ActionResult> Index()
        {
            
            return View(await _todoListService.GetAsync());
        }

        // GET: TodoList/Create
        public async Task<IActionResult> Create()
        {
            ToDoItem todo = new ToDoItem();
            try
            {
                List<string> result = (await _todoListService.GetAllUsersAsync()).ToList();

                TempData["UsersDropDown"] = result
                .Select(u => new SelectListItem
                {
                    Text = u
                }).ToList();
                return View(todo);
            }
            catch (WebApiMsalUiRequiredException ex)
            {
                var a = ex.Message;
                return Redirect(ex.Message);
            }
        }
        // POST: TodoList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Title,Owner")] ToDoItem todo)
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
        public async Task<ActionResult> Edit(int id, [Bind("Id,Title,Owner")] ToDoItem todo)
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
        public async Task<ActionResult> Delete(int id, [Bind("Id,Title,Owner")] ToDoItem todo)
        {
            await _todoListService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        public IActionResult AdminConsent()
        {
            var tenantId = User.GetTenantId();

            string adminConsent = "https://login.microsoftonline.com/" +
                       tenantId + "/v2.0/adminconsent?client_id=" + _ClientId
                       + "&redirect_uri=" + _RedirectUri + "&scope=" + _TodoListScope;
            return Redirect(adminConsent);
        }
    }
}