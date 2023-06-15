using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
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

        [Authorize(Policy = AuthorizationPolicies.AssignmentToGroupMemberGroupRequired)]
        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]
        public async Task<IActionResult> Index()
        {
            try
            {
                User me = await _graphServiceClient.Me.GetAsync();
                ViewData["Me"] = me;

                var photo = await _graphServiceClient.Me.Photo.GetAsync();
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
            catch (ServiceException svcex) when (svcex.IsMatch("ImageNotFound"))
            {
                //swallow
            }

            return View();
        }
    }
}