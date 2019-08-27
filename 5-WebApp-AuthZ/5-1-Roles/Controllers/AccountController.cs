using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Services;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class AccountController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly WebOptions webOptions;

        public AccountController(ITokenAcquisition tokenAcquisition,
                      IOptions<WebOptions> webOptionValue)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.webOptions = webOptionValue.Value;
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

        [MsalUiRequiredExceptionFilter(Scopes = new[] { GraphScopes.DirectoryReadAll })]
        [Authorize(Roles = AppRoles.DirectoryViewers)]
        public async Task<IActionResult> Groups()
        {
            string[] scopes = new[] { GraphScopes.DirectoryReadAll };

            GraphServiceClient graphServiceClient = GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(scopes);
                return result;
            }, webOptions.GraphApiUrl);

            var groups = await graphServiceClient.Me.MemberOf.Request().GetAsync();

            ViewData["Groups"] = groups.CurrentPage;

            return View();
        }
    }
}