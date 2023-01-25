using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;
using Microsoft.Graph;

namespace TodoListBFF.Controllers;

[Authorize]
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
            return BadRequest(ex.Message);
        }
    }
}

