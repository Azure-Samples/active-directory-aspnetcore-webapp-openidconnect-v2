using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Threading.Tasks;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    // This is how groups ids/names are used in the Authorize attribute
    //[Authorize(Roles = "8873daa2-17af-4e72-973e-930c94ef7549")] 
    public class UserProfileController : Controller
    {
        private readonly GraphServiceClient graphServiceClient;

        public UserProfileController(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient= graphServiceClient;
        }
        [Authorize(Policy = "GroupAdmin")]
        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]        
        public async Task<IActionResult> Index()
        {
            User me = await graphServiceClient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                var photo = await graphServiceClient.Me.Photo.Request().GetAsync();
                ViewData["Photo"] = photo;
            }
            catch
            {
                //swallow
            }
            return View();
        }
    }
}