### Process the CAE challenge from Microsoft Graph

To process the CAE challenge from Microsoft Graph, the controller actions need to extract it from the `wwwAuthenticate` header. It is returned when MS Graph rejects a seemingly valid Access tokens for MS Graph. For this you need to:

1. Inject and instance of `MicrosoftIdentityConsentAndConditionalAccessHandler` in the controller constructor. The beginning of the HomeController becomes:

   ```CSharp
   public class HomeController : Controller
   {
    private readonly ILogger<HomeController> _logger;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
    private string[] _graphScopes = new[] { "user.read" };
    public HomeController(ILogger<HomeController> logger,
                          IConfiguration configuration,
                          GraphServiceClient graphServiceClient,
                          MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
    {
      _logger = logger;
      _graphServiceClient = graphServiceClient;
      this._consentHandler = consentHandler;
      // Capture the Scopes for Graph that were used in the original request for an Access token (AT) for MS Graph as
      // they'd be needed again when requesting a fresh AT for Graph during claims challenge processing
      _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
    }
    
    // more code here
    ```
1. The process to handle CAE challenges from MS Graph comprises of the following steps:
    1. Catch a Microsoft Graph SDK's `ServiceException` and extract the required `claims`. This is done by wrapping the call to Microsoft Graph into a try/catch block that processes the challenge:
    ```CSharp
    currentUser = await _graphServiceClient.Me.GetAsync();
    ```
    1. Then redirect the user back to Azure AD with the new requested `claims`. Azure AD will use this `claims` payload to discern what or if any additional processing is required, example being the user needs to sign-in again or do multi-factor authentication.
  ```CSharp
    try
    {
        currentUser = await _graphServiceClient.Me.GetAsync();
    }
    // Catch CAE exception from Graph SDK
    catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
    {
      try
      {
        Console.WriteLine($"{svcex}");
        string claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);
        _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
        return new EmptyResult();
      }
      catch (Exception ex2)
      {
        _consentHandler.HandleException(ex2);
      }
    }        
  ```

   The `AuthenticationHeaderHelper` class is available from the `Helpers\AuthenticationHeaderHelper.cs file`.
