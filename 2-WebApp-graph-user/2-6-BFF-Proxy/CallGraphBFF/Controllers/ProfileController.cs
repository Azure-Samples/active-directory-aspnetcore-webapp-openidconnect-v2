using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Graph;

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
            User profile = await _graphServiceClient.Me
                .Request()
                .WithScopes("user.read")
                .GetAsync();

            return Ok(profile);
        }
        catch (ServiceException svcex) when (svcex.InnerException != null && svcex.InnerException.Message.Contains("MsalUiRequiredException"))
        {
            return Unauthorized("MsalUiRequiredException occurred. Please sign-in again.\n" + svcex.Message);
        }
        catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation"))
        {
            return Unauthorized("Continuous access evaluation challenge occurred. Please sign-in again.\n" + svcex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest("An error occurred while calling the downstream API\n" + ex.Message);
        }
    }
}

