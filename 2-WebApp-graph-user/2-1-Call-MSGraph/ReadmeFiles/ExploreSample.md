## Explore the sample

1. Open your web browser and make a request to the app at url `https://localhost:44321`. The app immediately attempts to authenticate you via the Microsoft identity platform. Sign in with a work or school account.
2. Provide consent to the screen presented.
3. Click on the **Profile** link on the top menu. The web app will make a call to the Microsoft Graph `/me` endpoint. You should see information about the signed-in user's account, as well as its picture, if these values are set in the account's profile.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About The code

1. In this aspnetcore web project, first the packages `Microsoft.Identity.Web`,  `Microsoft.Identity.Web.UI` and `Microsoft.Identity.Web.GraphServiceClient` were added from NuGet. These libraries are used to simplify the process of signing-in a user and acquiring tokens for Microsoft Graph.

2. Starting with the **Startup.cs** file :

   - at the top of the file, the following two using directives were added:

     ```CSharp
      using Microsoft.Identity.Web;
      using Microsoft.Identity.Web.UI;
      ```

   - in the `ConfigureServices` method, the following code was added, replacing any existing `AddAuthentication()` code:

    ```CSharp

    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
        .AddInMemoryTokenCaches();

    ```

     `AddMicrosoftIdentityWebApp()` enables your application to sign-in a user with the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts (if required).

    `EnableTokenAcquisitionToCallDownstreamApi()` and `AddMicrosoftGraph` adds support to call Microsoft Graph. This lines ensures that the GraphAPIService benefits from the optimized `HttpClient` management by ASP.NET Core.

3. In the `Controllers\HomeController.cs` file, the following code is added to allow calling MS Graph:

 ```CSharp
   private readonly ILogger<HomeController> _logger;
   private readonly GraphServiceClient _graphServiceClient;
  
   private readonly GraphServiceClient _graphServiceClient;
   public HomeController(ILogger<HomeController> logger,
                      IConfiguration configuration,
                      GraphServiceClient graphServiceClient)
   {
    _logger = logger;
    _graphServiceClient = graphServiceClient;
    this._consentHandler = consentHandler;
   }
   ```

4. In the `Profile()` action we make a call to the Microsoft Graph `/me` endpoint. In case a token cannot be acquired, a challenge is attempted to re-sign-in the user, and have them consent to the requested scopes. This is expressed declaratively by the `AuthorizeForScopes`attribute. This attribute is part of the `Microsoft.Identity.Web` project and automatically manages incremental consent.

   ```CSharp
   [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
   public async Task<IActionResult> Profile()
   {
    var me = await _graphServiceClient.Me.GetAsync();
    ViewData["Me"] = me;

    try
    {
        // Get user photo
        using (var photoStream = await _graphServiceClient.Me.Photo.Content.GetAsync())
        {
            byte[] photoByte = ((MemoryStream)photoStream).ToArray();
            ViewData["Photo"] = Convert.ToBase64String(photoByte);
        }
    }
    catch (System.Exception)
    {
        ViewData["Photo"] = null;
    }

    return View();
   }
   ```

5. Update `launchSetting.json`. Change the following values in the `Properties\launchSettings.json` file to ensure that you start your web app from `https://localhost:44321`:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - update the `applicationUrl` property to `https://localhost:44321`
