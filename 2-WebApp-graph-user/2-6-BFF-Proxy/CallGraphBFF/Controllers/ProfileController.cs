using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;

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
        catch (MsalUiRequiredException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest("An error occurred while calling the downstream API\n" + ex.Message);
        }
    }
}

