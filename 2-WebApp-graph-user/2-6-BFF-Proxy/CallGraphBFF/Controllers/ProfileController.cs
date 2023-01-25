using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Graph;

namespace TodoListBFF.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[AuthorizeForScopes(Scopes = new string[] { "user.read" })]
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
            User profile = await _graphServiceClient.Me
                .Request()
                .WithScopes("user.read")
                .GetAsync();

            return Ok(profile);
        }
        catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation"))
        {
            return Unauthorized("Continuous access evaluation challenge occurred\n" + svcex.Message);
        }
        catch (MsalUiRequiredException ex)
        {
            return Unauthorized("MsalUiRequiredException occurred while calling the downstream API\n" + ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest("An error occurred while calling the downstream API\n" + ex.Message);
        }
    }
}

