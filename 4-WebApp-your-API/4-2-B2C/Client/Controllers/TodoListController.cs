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
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TodoListClient.Models;

namespace TodoListClient.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;
        private readonly string WebApiBaseAddress = string.Empty;

        public TodoListController(ITokenAcquisition tokenAcquisition, IConfiguration configuration)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            WebApiBaseAddress = _configuration["TodoList:TodoListBaseAddress"];
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        public async Task<IActionResult> Index()
        {
            HttpClient client = await PrepareAuthenticatedClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{WebApiBaseAddress}/api/todolist");
            HttpResponseMessage response = await client.SendAsync(request);

            IEnumerable<TodoItem> todolist = await HandleTodoListResponseOf<IEnumerable<TodoItem>>(response);
            return View(todolist);
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        public async Task<IActionResult> Details(int id)
        {
            HttpClient client = await PrepareAuthenticatedClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{WebApiBaseAddress}/api/todolist/{id}");
            HttpResponseMessage response = await client.SendAsync(request);

            TodoItem todoItem = await HandleTodoListResponseOf<TodoItem>(response);
            return View(todoItem);
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        public IActionResult Create()
        {
            TodoItem todo = new TodoItem() { Owner = HttpContext.User.Identity.Name };
            return View(todo);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoItem todo)
        {
            HttpClient client = await PrepareAuthenticatedClient();
            var jsonRequest = JsonConvert.SerializeObject(todo);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");

            var response = await client.PostAsync($"{WebApiBaseAddress}/api/todolist", jsoncontent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("Index");
            }
            else
            {
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
            }
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        public async Task<IActionResult> Edit(int id)
        {
            HttpClient client = await PrepareAuthenticatedClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{WebApiBaseAddress}/api/todolist/{id}");
            HttpResponseMessage response = await client.SendAsync(request);

            TodoItem todoItem = await HandleTodoListResponseOf<TodoItem>(response);

            if (todoItem == null)
            {
                return NotFound();
            }

            return View(todoItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TodoItem todo)
        {
            HttpClient client = await PrepareAuthenticatedClient();
            var jsonRequest = JsonConvert.SerializeObject(todo);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");

            var response = await client.PatchAsync($"{WebApiBaseAddress}/api/todolist/{todo.Id}", jsoncontent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("Index");
            }
            else
            {
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
            }
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        public async Task<IActionResult> Delete(int id)
        {
            HttpClient client = await PrepareAuthenticatedClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{WebApiBaseAddress}/api/todolist/{id}");
            HttpResponseMessage response = await client.SendAsync(request);

            TodoItem todoItem = await HandleTodoListResponseOf<TodoItem>(response);

            if (todoItem == null)
            {
                return NotFound();
            }

            return View(todoItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, TodoItem todoItem)
        {
            HttpClient client = await PrepareAuthenticatedClient();
            var response = await client.DeleteAsync($"{WebApiBaseAddress}/api/todolist/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("Index");
            }
            else
            {
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
            }
        }

        private async Task<HttpClient> PrepareAuthenticatedClient()
        {
            var todoListScope = _configuration["TodoList:TodoListScope"];
            var accessToken = await this._tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(new[] { todoListScope });
            Debug.WriteLine($"access token-{accessToken}");
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        private async Task<T> HandleTodoListResponseOf<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                T todo = JsonConvert.DeserializeObject<T>(content);
                return todo;
            }
            else
            {
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
            }
        }
    }
}