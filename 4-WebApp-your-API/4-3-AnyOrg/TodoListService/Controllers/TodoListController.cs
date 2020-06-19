using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TodoListAPI.Models;
using Microsoft.Identity.Web.Resource;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using Microsoft.Graph;
using System.Net;

namespace TodoListAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoListController : ControllerBase
    {
        // The Web API will only accept tokens 1) for users, and 
        // 2) having the access_as_user scope for this API
        static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };

        private readonly TodoContext _context;
        private ITokenAcquisition _tokenAcquisition;
     
        public TodoListController(TodoContext context,ITokenAcquisition tokenAcquisition)
        {
            _context = context;
            _tokenAcquisition = tokenAcquisition;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            try
            {
                Microsoft.Graph.User user = new User();
                await _context.TodoItems.ToListAsync();
            }
            catch(Exception ex)
            {
                var a = ex.Message;
            }
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
 
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }
        [HttpGet("getallusers")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllUsers()
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            List<string> Users = await CallGraphApiOnBehalfOfUser();
            if (Users == null)
            {
                return NotFound();
            }

            return Users;
        }
        // PUT: api/TodoItems/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(int id, TodoItem todoItem)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

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
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            var random = new Random();
            todoItem.Id = random.Next();

            
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return Ok(todoItem);
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(int id)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

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
        public async Task<List<string>> CallGraphApiOnBehalfOfUser()
        {
            string[] scopes = { "user.read.all" };

            // we use MSAL.NET to get a token to call the API On Behalf Of the current user
            try
            {
                List<string> userList = new List<string>();
                string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                IEnumerable<User> users = await CallGraphApiOnBehalfOfUser(accessToken);
                userList = users.Select(x => x.UserPrincipalName).ToList();
                return userList;
            }
            catch (MsalUiRequiredException ex)
            {
                _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader(scopes, ex);
                throw (ex);
            }
        }
        private static async Task<IEnumerable<User>> CallGraphApiOnBehalfOfUser(string accessToken)
        {
            // Call the Graph API and retrieve the user's profile.
            GraphServiceClient graphServiceClient = GetGraphServiceClient(accessToken);
            IGraphServiceUsersCollectionPage users = await graphServiceClient.Users.Request()
                                                      .Filter($"accountEnabled eq true")
                                                      .Select("id, userPrincipalName")
                                                      .GetAsync();
            if (users != null)
            {

                return users;
            }
            throw new Exception();
        }
        /// <summary>
        /// Prepares the authenticated client.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        private static GraphServiceClient GetGraphServiceClient(string accessToken)
        {
            try
            {
                /***
                //Microsoft Azure AD Graph API endpoint,
                'https://graph.microsoft.com'   Microsoft Graph global service
                'https://graph.microsoft.us' Microsoft Graph for US Government
                'https://graph.microsoft.de' Microsoft Graph Germany
                'https://microsoftgraph.chinacloudapi.cn' Microsoft Graph China
                 ***/

                string graphEndpoint = "https://graph.microsoft.com/v1.0/";
                GraphServiceClient graphServiceClient = new GraphServiceClient(graphEndpoint,
                                                                     new DelegateAuthenticationProvider(
                                                                         async (requestMessage) =>
                                                                         {
                                                                             await Task.Run(() =>
                                                                             {
                                                                                 requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                                                                             });
                                                                         }));
                return graphServiceClient;
            }
            catch (Exception ex)
            {
                return null;   
            }
        }
    }
}
