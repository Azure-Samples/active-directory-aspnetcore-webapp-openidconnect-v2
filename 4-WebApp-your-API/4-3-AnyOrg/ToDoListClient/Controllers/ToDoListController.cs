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

namespace ToDoListClient.Controllers
{
    public class ToDoListController : Controller
    {
        private IToDoListService _todoListService;

        public ToDoListController(IToDoListService todoListService)
        {
            _todoListService = todoListService;
        }
        // GET: TodoList
        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        public async Task<ActionResult> Index()
        {
            
            return View(await _todoListService.GetAsync());
        }

        // GET: TodoList/Create
        public async Task<IActionResult> Create()
        {
            ToDoItem todo = new ToDoItem();
            TempData["UsersDropDown"] = (await _todoListService.GetAllUsersAsync())
                .Select(u => new SelectListItem
                {
                    Text = u
                }).ToList();
            return View(todo);
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
            string redirectUrl = Request.Scheme + Uri.SchemeDelimiter + Request.Host.Value;
            var tenantId = User.Claims.First(x => x.Type == "http://schemas.microsoft.com/identity/claims/tenantid" || x.Type=="tid").Value;
          
            string adminConsent = _todoListService.GetAdminConsentEndpoint(tenantId, redirectUrl);
            
            return Redirect(adminConsent);
        }
    }
}