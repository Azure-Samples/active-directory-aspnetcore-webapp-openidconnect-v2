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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using TodoListService.Models;
using Constants = TodoListService.Models.Constants;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        private readonly IHttpContextAccessor _contextAccessor;

        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IOptions<MicrosoftGraphOptions> _graphOptions;
        private IConfiguration _configuration;

        public TodoListController(IConfiguration configuration, IHttpContextAccessor contextAccessor, ITokenAcquisition tokenAcquisition, GraphServiceClient graphServiceClient, IOptions<MicrosoftGraphOptions> graphOptions)
        {
            this._contextAccessor = contextAccessor;
            _tokenAcquisition = tokenAcquisition;
            _graphServiceClient = graphServiceClient;
            _graphOptions = graphOptions;
            _configuration = configuration;

            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new Todo() { Id = 1, Owner = $"{this._contextAccessor.HttpContext.User.GetDisplayName()}", Title = "Pick up groceries" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = $"{this._contextAccessor.HttpContext.User.GetDisplayName()}", Title = "Finish invoice report" });
            }
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<Todo> Get()
        {
            string owner = this._contextAccessor.HttpContext.User.GetDisplayName();
            return TodoStore.Values.Where(x => x.Owner == owner);
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        public Todo Get(int id)
        {
            return TodoStore.Values.FirstOrDefault(t => t.Id == id);
        }

        [AuthorizeForScopes(Scopes = new[] { Constants.scopeRequiredByApi })]
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            EnsureUserHasElevatedScope();
            TodoStore.Remove(id);
        }

        // POST api/values
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            int id = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            //Todo todonew = new Todo() { Id = id, Owner = HttpContext.User.Identity.Name, Title = todo.Title };
            //TodoStore.Add(id, todonew);

            // This is a synchronous call, so that the clients know, when they call Get, that the
            // call to the downstream API (Microsoft Graph) has completed.
            try
            {
                string owner = this._contextAccessor.HttpContext.User.GetDisplayName();
                User user = _graphServiceClient.Me.Request().GetAsync().GetAwaiter().GetResult();
                string title = string.IsNullOrWhiteSpace(user.UserPrincipalName) ? todo.Title : $"{todo.Title} ({user.UserPrincipalName})";
                TodoStore.Add(id, new Todo { Owner = owner, Title = title });
            }
            catch (MsalException ex)
            {
                HttpContext.Response.ContentType = "text/plain";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                HttpContext.Response.WriteAsync("An authentication error occurred while acquiring a token for downstream API\n" + ex.ErrorCode + "\n" + ex.Message);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is MicrosoftIdentityWebChallengeUserException challengeException)
                {
                    _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(_graphOptions.Value.Scopes.Split(' '),
                       challengeException.MsalUiRequiredException);
                }
                else
                {
                    HttpContext.Response.ContentType = "text/plain";
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    HttpContext.Response.WriteAsync("An error occurred while calling the downstream API\n" + ex.Message);
                }
            }

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

        public void EnsureUserHasElevatedScope()
        {
            HttpContext context = this.HttpContext;
            string elevatedScope = _configuration.GetSection("MyApiSettings").GetSection("ElevatedScopeName").Value;
            string authenticationContextClassReferencesClaim = "acrs";

            if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
            {
                throw new ArgumentNullException("No Usercontext is available to pick claims from");
            }

            // Attempt with Scp claim
            Claim acrsClaim = context.User.FindFirst(authenticationContextClassReferencesClaim);

            if (acrsClaim == null || acrsClaim.Value != elevatedScope)
            {
                // claims={"id_token":{"acrs":{"essential":true, "value":"urn:microsoft:req1"}}}
                // WWW-Authenticate: Bearer realm="", authorization_uri="https://login.microsoftonline.com/common/oauth2/authorize", client_id="00000003-0000-0000-c000-000000000000", error="insufficient_claims", claims="eyJhY2Nlc3NfdG9rZW4iOnsiYWNycyI6eyJlc3NlbnRpYWwiOnRydWUsInZhbHVlIjoidXJuOm1pY3Jvc29mdDpyZXExIn19fQ==", cc_type="authcontext""
                string clientId = _configuration.GetSection("AzureAd").GetSection("ClientId").Value;
                var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"urn:microsoft:req1\"}}}"));

                context.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", client_id=\""+clientId+"\", error=\"insufficient_claims\", claims=\""+ base64str + "\", cc_type=\"authcontext\"");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                string message = string.Format(CultureInfo.InvariantCulture, "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.");
                context.Response.WriteAsync(message);
                context.Response.CompleteAsync();
            }
        }

        public string stringToBase64ByteArray(String input)
        {
            byte[] ret = System.Text.Encoding.Unicode.GetBytes(input);
            string s = Convert.ToBase64String(ret);
            //ret = System.Text.Encoding.Unicode.GetBytes(s);
            return s;
        }
    }
}