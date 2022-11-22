using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.Identity.Web;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Services;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class AccountController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;

        private readonly GraphServiceClient _graphServiceClient;

        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private string[] _graphScopes;
        private readonly GraphHelper _graphHelper;

        public AccountController(IHttpContextAccessor httpContextAccessor,
            ITokenAcquisition tokenAcquisition,
                      GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
            IConfiguration configuration)
        {
            this.tokenAcquisition = tokenAcquisition;
            this._graphServiceClient = graphServiceClient;
            this._consentHandler = consentHandler;
            _httpContextAccessor = httpContextAccessor;
            //string[] graphScopes = configuration.GetValue<string>("GraphAPI:Scopes")?.Split(' ');

            this._graphHelper = new GraphHelper(this._httpContextAccessor.HttpContext, new[] { GraphScopes.DirectoryReadAll });
        }

        /// <summary>
        /// AspNet core's default AuthorizeAttribute redirects to '/Account/AccessDenied' when it processes the Http code 403 (Unauthorized)
        /// Instead of implementing an Attribute class of our own to construct a redirect Url, we'd just implement our own to show an error message of
        /// our choice to the user.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Fetches all the groups a user is assigned to.  This method requires the signed-in user to be assigned to the 'DirectoryViewers' approle.
        /// </summary>
        /// <returns></returns>
        [AuthorizeForScopes(Scopes = new[] { GraphScopes.DirectoryReadAll })]
        [Authorize(Policy = AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired)]
        public async Task<IActionResult> Groups()
        {
            //string[] scopes = new[] { GraphScopes.DirectoryReadAll };

            //GraphServiceClient graphServiceClient = GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            //{
            //    string result = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            //    return result;
            //}, webOptions.GraphApiUrl);

            //var groups = await _graphServiceClient.Me.MemberOf.Request().GetAsync();

            //ViewData["Groups"] = groups.CurrentPage;
            ViewData["Groups"] = await _graphHelper.GetMemberOfAsync();

            return View();
        }
    }
}