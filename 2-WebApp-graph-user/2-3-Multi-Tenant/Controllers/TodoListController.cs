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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using System.Linq;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services;
using WebApp_OpenIDConnect_DotNet.Utils;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private readonly ITodoItemService _todoItemService;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IMSGraphService _msGraphService;

        public TodoListController(ITodoItemService todoItemService, ITokenAcquisition tokenAcquisition, IMSGraphService msGraphService)
        {
            _todoItemService = todoItemService;
            _tokenAcquisition = tokenAcquisition;
            _msGraphService = msGraphService;
        }

        public async Task<IActionResult> Index(bool showAllFilter)
        {
            ViewData["ShowAllFilter"] = showAllFilter;

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
                UserName = User.GetDisplayName()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _todoItemService.Create(model.Text, User);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [AuthorizeForScopes(Scopes = new string[] { GraphScope.UserReadAll })]
        public async Task<IActionResult> Edit(int id)
        {
            TodoItem todoItem = await _todoItemService.Get(id, User);

            if (todoItem == null)
            {
                TempData["ErrorMessage"] = "Item not found";
                return RedirectToAction("Error", "Home");
            }

            var userTenant = User.GetTenantId();

            // Acquiring token for graph in the signed-in users tenant, so it can be used to retrieve all the users from their tenant
            var graphAccessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { GraphScope.UserReadAll }, userTenant);

            TempData["UsersDropDown"] = (await _msGraphService.GetUsersAsync(graphAccessToken))
                .Select(u => new SelectListItem
                {
                    Text = u.UserPrincipalName,
                    Value = u.Id
                }).ToList();

            return View(todoItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TodoItem todoItem)
        {
            await _todoItemService.Edit(todoItem, User);
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