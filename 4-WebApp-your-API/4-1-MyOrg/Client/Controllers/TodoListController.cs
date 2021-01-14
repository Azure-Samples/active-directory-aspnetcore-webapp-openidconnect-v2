using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TodoListClient.Infrastructure;
using TodoListClient.Services;
using TodoListService.Models;

namespace TodoListClient.Controllers
{
    public class TodoListController : Controller
    {
        private ITodoListService _todoListService;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public TodoListController(ITodoListService todoListService,
                          MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _todoListService = todoListService;
            this._consentHandler = consentHandler;
        }

        // GET: TodoList
        [AuthorizeForScopes(ScopeKeySection = "TodoList:TodoListScope")]
        public async Task<ActionResult> Index()
        {
            return View(await _todoListService.GetAsync());
        }

        // GET: TodoList/Details/5
        public async Task<ActionResult> Details(int id)
        {
            return View(await _todoListService.GetAsync(id));
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

            await _todoListService.AddAsync(todo);

            return RedirectToAction("Index");
        }

        // GET: TodoList/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            Todo todo = await this._todoListService.GetAsync(id);

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
            await _todoListService.EditAsync(todo);
            return RedirectToAction("Index");
        }

        // GET: TodoList/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            Todo todo = await this._todoListService.GetAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: TodoList/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            try
            {
                await _todoListService.DeleteAsync(id);
            }
            catch (TodolistServiceException hex)
            {
                if (hex.StatusCode == System.Net.HttpStatusCode.Unauthorized && hex.Headers.WwwAuthenticate.Any())
                {
                    AuthenticationHeaderValue bearer = hex.Headers.WwwAuthenticate.First(v => v.Scheme == "Bearer");
                    IEnumerable<string> parameters = bearer.Parameter.Split(',').Select(v => v.Trim()).ToList();
                    var errorValue = GetParameterValue(parameters, "error");

                    try
                    {
                        if (null != errorValue && "insufficient_claims" == errorValue)
                        {
                            var claimChallengeParameter = GetParameterValue(parameters, "claims");
                            if (null != claimChallengeParameter)
                            {
                                var claimChallengebase64Bytes = System.Convert.FromBase64String(claimChallengeParameter);
                                var claimChallenge = System.Text.Encoding.UTF8.GetString(claimChallengebase64Bytes);

                                _consentHandler.ChallengeUser(new string[] { "user.read Sites.Read.All" }, claimChallenge);
                            }
                            return new EmptyResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _consentHandler.HandleException(ex);
                    }
                }
                Console.WriteLine(hex.Message);
            }

            return RedirectToAction("Index");
        }

        private string GetParameterValue(IEnumerable<string> parameters, string keyToLook)
        {
            char[] pc = new char[2] { ' ', '\"' };

            if (parameters.Count() > 0)
            {
                foreach (string parameter in parameters)
                {
                    int i = parameter.IndexOf('=');

                    if (i > 0 && i < (parameter.Length - 1))
                    {
                        string key = parameter.Substring(0, i).Trim(pc).ToLower();
                        string value = parameter.Substring(i + 1).Trim(pc);

                        if (key==keyToLook)
                        {
                            return value;
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}