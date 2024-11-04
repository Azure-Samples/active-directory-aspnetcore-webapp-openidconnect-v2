using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace TodoListBFF.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class ProfileController : Controller
{
    private readonly GraphServiceClient _graphServiceClient;

    public ProfileController(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    [HttpGet]
    public async Task<ActionResult<User>> GetProfile()
    {
        try
        {
            User? profile = await _graphServiceClient.Me
                .GetAsync();

            return Ok(profile);
        }
        catch (ServiceException svcex) 
        when (svcex.InnerException is MicrosoftIdentityWebChallengeUserException)
        {
            return Unauthorized("MicrosoftIdentityWebChallengeUserException occurred\n" + svcex.Message);
        }
        catch (ServiceException svcex) 
        when (svcex.Message.Contains("Continuous access evaluation"))
        {
            string claimsChallenge = WwwAuthenticateParameters
                .GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);

            // Set the claims challenge string to session, which will be used during the next login request
            HttpContext.Session.SetString("claimsChallenge", claimsChallenge);

            return Unauthorized("Continuous access evaluation resulted in claims challenge\n" + svcex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest("An error occurred while calling the downstream API\n" + ex.Message);
        }
    }
}

