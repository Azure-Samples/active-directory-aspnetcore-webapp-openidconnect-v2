using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    // This is how groups ids/names are used in the Authorize attribute
    //[Authorize(Roles = "8873daa2-17af-4e72-973e-930c94ef7549")] 
    public class UserProfileController : Controller
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private string[] _graphScopes;

        public UserProfileController(
            IConfiguration configuration, 
            GraphServiceClient graphServiceClient,
            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {

            _consentHandler = consentHandler;
            _graphServiceClient = graphServiceClient;
            _graphScopes = configuration.GetValue<string>("GraphAPI:Scopes")?.Split(' ');
        }

        [Authorize(Policy = AuthorizationPolicies.AssignmentToGroupAdminGroupRequired)]
        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]
        public async Task<IActionResult> Index()
        {
            User me = await _graphServiceClient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                var photo = await _graphServiceClient.Me.Photo.Request().GetAsync();
                ViewData["Photo"] = photo;
            }
            // Catch CAE exception from Graph SDK
            catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                try
                {
                    string claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);
                    _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
                    return new EmptyResult();
                }
                catch (Exception ex2)
                {
                    _consentHandler.HandleException(ex2);
                }
            }
            catch
            {
                //swallow
            }
            return View();
        }
    }
}