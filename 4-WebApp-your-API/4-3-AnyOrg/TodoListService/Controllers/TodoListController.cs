using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ToDoListService.Models;
using Microsoft.Identity.Web.Resource;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace ToDoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoListController : ControllerBase
    {
        private readonly TodoContext _context;
        private ITokenAcquisition _tokenAcquisition;
        private readonly string[] _graphScopes;
        private readonly string _graphApiUrl;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly GraphServiceClient _graphServiceClient;

        public TodoListController(TodoContext context, ITokenAcquisition tokenAcquisition, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tokenAcquisition = tokenAcquisition;
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            _graphApiUrl = configuration.GetValue<string>("DownstreamApi:GraphApiUrl");

            var services = httpContextAccessor.HttpContext.RequestServices;

            this._graphServiceClient = (GraphServiceClient)services.GetService(typeof(GraphServiceClient));
            if (this._graphServiceClient == null) throw new NullReferenceException("The GraphServiceClient has not been added to the services collection during the ConfigureServices()");

            this._consentHandler = (MicrosoftIdentityConsentAndConditionalAccessHandler)services.GetService(typeof(MicrosoftIdentityConsentAndConditionalAccessHandler));
            if (this._consentHandler == null) throw new NullReferenceException("The MicrosoftIdentityConsentAndConditionalAccessHandler has not been added to the services collection during the ConfigureServices()");

        }

        // GET: api/TodoItems
        [HttpGet]
        [RequiredScope("ToDoList.Read")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            string userTenantId = HttpContext.User.GetTenantId();
            var signedInUser = HttpContext.User.GetDisplayName();
            try
            {
                await _context.TodoItems.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return await _context.TodoItems.Where
                (x => x.TenantId == userTenantId && (x.AssignedTo == signedInUser || x.Assignedby == signedInUser)).ToListAsync();
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        [RequiredScope("ToDoList.Read")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }
        [HttpGet("getallusers")]
        [RequiredScope("ToDoList.Read")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllUsers()
        {
            try
            {
                List<string> Users = await CallGraphApiOnBehalfOfUser();
                return Users;
            }
            catch (MsalUiRequiredException ex)
            {
                HttpContext.Response.ContentType = "text/plain";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await HttpContext.Response.WriteAsync("An authentication error occurred while acquiring a token for downstream API\n" + ex.ErrorCode + "\n" + ex.Message);
            }

            return null;
        }
        // PUT: api/TodoItems/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        [RequiredScope("ToDoList.ReadWrite")]
        public async Task<IActionResult> PutTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(todoItem);
        }

        // POST: api/TodoItems
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        [RequiredScope("ToDoList.ReadWrite")]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            var random = new Random();
            todoItem.Id = random.Next();


            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return Ok(todoItem);
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        [RequiredScope("ToDoList.ReadWrite")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return todoItem;
        }

        private bool TodoItemExists(int id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }

        private async Task<List<string>> CallGraphApiOnBehalfOfUser()
        {
            // we use MSAL.NET to get a token to call the API On Behalf Of the current user
            try
            {
                // Call the Graph API and retrieve the user's profile.
                IGraphServiceUsersCollectionPage users =
                await CallGraphWithCAEFallback(
                    async () =>
                    {
                        return await _graphServiceClient.Users.Request()
                                                              .Filter($"accountEnabled eq true")
                                                              .Select("id, userPrincipalName")
                                                              .GetAsync();
                    }
                );

                if (users != null)
                {
                    return users.Select(x => x.UserPrincipalName).ToList();
                }
                throw new Exception();
            }
            catch (MsalUiRequiredException ex)
            {
                _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader(_graphScopes, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Calls a Microsoft Graph API, but wraps and handle a CAE exception, if thrown
        /// </summary>
        /// <typeparam name="T">The type of the object to return from MS Graph call</typeparam>
        /// <param name="graphAPIMethod">The graph API method to call.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unknown error just occurred. Message: {ex.Message}</exception>
        /// <autogeneratedoc />
        private async Task<T> CallGraphWithCAEFallback<T>(Func<Task<T>> graphAPIMethod)
        {
            try
            {
                return await graphAPIMethod();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                try
                {
                    // Get challenge from response of Graph API
                    var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(ex.ResponseHeaders);

                    _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
                }
                catch (Exception ex2)
                {
                    _consentHandler.HandleException(ex2);
                }

                return default;
            }
        }
    }
}
