using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly IMSGraphService graphApiOperations;

        public UserProfileController(ITokenAcquisition tokenAcquisition,
                              IMSGraphService MSGraphService)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.graphApiOperations = MSGraphService;
        }

        [MsalUiRequiredExceptionFilter(Scopes = new[] { Constants.ScopeUserRead, Constants.ScopeDirectoryReadAll })]
        public async Task<IActionResult> Index()
        {
            string accessToken = await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, new[] { Constants.ScopeUserRead, Constants.ScopeDirectoryReadAll });

            User me = await graphApiOperations.GetMeAsync(accessToken);
            var photo = await graphApiOperations.GetMyPhotoAsync(accessToken);
            IList<Group> groups = await graphApiOperations.GetMyMemberOfGroupsAsync(accessToken);

            ViewData["Me"] = me;
            ViewData["Photo"] = photo;
            ViewData["Groups"] = groups;

            return View();
        }
    }
}