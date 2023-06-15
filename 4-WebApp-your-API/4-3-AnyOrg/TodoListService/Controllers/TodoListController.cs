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
using System.Security.Claims;
using ToDoListService.Extensions;

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
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly GraphServiceClient _graphServiceClient;
        IHttpContextAccessor _httpContextAccessor;
        private readonly string _userTenantId;
        private readonly string _signedInUser;

        private const string _todoListReadScope = "ToDoList.Read";
        private const string _todoListReadWriteScope = "ToDoList.ReadWrite";
        private const string _todoListReadAllPermission = "ToDoList.Read.All";
        private const string _todoListReadWriteAllPermission = "ToDoList.ReadWrite.All";

        public TodoListController(TodoContext context, ITokenAcquisition tokenAcquisition, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tokenAcquisition = tokenAcquisition;
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            _httpContextAccessor = httpContextAccessor;

            var services = _httpContextAccessor.HttpContext.RequestServices;

            this._graphServiceClient = (GraphServiceClient)services.GetService(typeof(GraphServiceClient));
            if (this._graphServiceClient == null) throw new NullReferenceException("The GraphServiceClient has not been added to the services collection during the ConfigureServices()");

            this._consentHandler = (MicrosoftIdentityConsentAndConditionalAccessHandler)services.GetService(typeof(MicrosoftIdentityConsentAndConditionalAccessHandler));
            if (this._consentHandler == null) throw new NullReferenceException("The MicrosoftIdentityConsentAndConditionalAccessHandler has not been added to the services collection during the ConfigureServices()");

            _userTenantId = _httpContextAccessor.HttpContext.User.GetTenantId();
            _signedInUser = _httpContextAccessor.HttpContext.User.GetDisplayName();
        }

        // GET: api/TodoItems
        [HttpGet]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }
            )]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            try
            {
                // this is a request for all ToDo list items of a certain user.
                if (!IsAppOnlyToken())
                {
                    return await _context.TodoItems.Where(x => x.TenantId == _userTenantId && (x.AssignedTo == _signedInUser || x.Assignedby == _signedInUser)).ToArrayAsync();
                }

                // Its an app calling with app permissions, so return all items across all users
                return await _context.TodoItems.Where(x => x.TenantId == _userTenantId).ToArrayAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission })]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            try
            {
                TodoItem todoItem;

                if (!IsAppOnlyToken())
                {
                    todoItem = (await _context.TodoItems.WhereAsync(x => x.TenantId == _userTenantId && x.Id == id && (x.AssignedTo == _signedInUser || x.Assignedby == _signedInUser))).FirstOrDefault();
                }
                else
                {
                    todoItem = (await _context.TodoItems.WhereAsync(x => x.TenantId == _userTenantId && x.Id == id)).FirstOrDefault();
                }

                if (todoItem == null)
                {
                    return NotFound();
                }

                return todoItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("getallgraphusers")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission })]
        public async Task<ActionResult<IEnumerable<string>>> GetAllGraphUsers()
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
        [HttpPatch("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public async Task<IActionResult> UpdateTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }            

            try
            {
                if (
                    //non application call and the user is (assigned and/or assigned)
                    (!IsAppOnlyToken() && (todoItem.AssignedTo == _signedInUser || todoItem.Assignedby == _signedInUser)) 
                    
                    //application call
                    || IsAppOnlyToken()
                )
                {
                    _context.Entry(todoItem).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                } else 
                {
                    return Unauthorized("Only assigned user, ToDo Item assignee or an application with 'Write' permission are authorized to modify their ToDo Items");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IsTodoItemExists(id))
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
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {
            var random = new Random();
            todoItem.Id = random.Next();


            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
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

        private bool IsTodoItemExists(int id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }

        private async Task<List<string>> CallGraphApiOnBehalfOfUser()
        {
            // we use MSAL.NET to get a token to call the API On Behalf Of the current user
            try
            {
                // Call the Graph API and retrieve the user's profile.
                var users =
                await CallGraphWithCAEFallback(
                    async () =>
                    {
                        return await _graphServiceClient.Users.GetAsync(r =>
                                                              {
                                                                  r.QueryParameters.Filter = "accountEnabled eq true";
                                                                  r.QueryParameters.Select = new string[] { "id", "userPrincipalName" };
                                                              }
                                                              );

                    }
                );

                if (users != null)
                {
                    return users.Value.Select(x => x.UserPrincipalName).ToList();
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

        /// <summary>
        /// Indicates of the AT presented was for an app-only token or not.
        /// </summary>
        /// <returns></returns>
        private bool IsAppOnlyToken()
        {
            // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
            //
            // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims

            if (GetCurrentClaimsPrincipal() != null)
            {
                return GetCurrentClaimsPrincipal().Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
            }

            return false;
        }

        /// <summary>
        /// returns the current claimsPrincipal (user/Client app) dehydrated from the Access token
        /// </summary>
        /// <returns></returns>
        private ClaimsPrincipal GetCurrentClaimsPrincipal()
        {
            // Irrespective of whether a user signs in or not, the AspNet security middle-ware dehydrates the claims in the
            // HttpContext.User.Claims collection

            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User != null)
            {
                return _httpContextAccessor.HttpContext.User;
            }

            return null;
        }
    }
}
