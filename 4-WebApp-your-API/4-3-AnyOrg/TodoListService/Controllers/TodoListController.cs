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
using System.Net.Http.Headers;
using Microsoft.Graph;
using System.Net;

namespace ToDoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [RequiredScope("access_as_user")]
    [ApiController]
    public class TodoListController : ControllerBase
    {
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
            string userTenantId = HttpContext.User.GetTenantId();
            var signedInUser = HttpContext.User.GetDisplayName();
            try
            {
                await _context.TodoItems.ToListAsync();
            }
            catch(Exception)
            {
                throw;
            }
            return await _context.TodoItems.Where
                (x => x.TenantId == userTenantId && (x.AssignedTo == signedInUser || x.Assignedby== signedInUser)).ToListAsync();
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
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
                await _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(scopes, ex);
                throw ex;
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
