using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Services;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp_OpenIDConnect_DotNet.Options;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

var azureAdOptions = Options
    .Create(builder.Configuration.GetSection("AzureAd").Get<AzureAdOptions>());

var confidentialClientService = new ConfidentialClientApplicationService(azureAdOptions);

builder.Services.AddSingleton<IConfidentialClientApplicationService>(confidentialClientService);

builder.Services.AddSession();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        // Needed to make PKCE possible.
        //
        // https://datatracker.ietf.org/doc/html/rfc7636 
        options.ResponseType = OpenIdConnectResponseType.Code;

        // Scopes need to be added in to get proper claims for user.
        var apiScopes = builder.Configuration.GetSection("DownstreamApi:Scopes").Value;

        foreach (var scope in apiScopes.Split(' '))
        {
            options.Scope.Add(scope);
        }

        // This part of the flow needs to be customized for a hybrid flow. In most flows the OnAuthorizationCodeReceived 
        // receives an authorization code from the authorization end point and exchanges it for an access token.
        // This event will still request an access token when it receives an authorization code but it will also
        // request an access code that will be passed to the front-end so it can be exchanged for an access token.
        // The access code is passed to the front-end via the 'Microsoft.Identity.Hybrid.Authentication' session value.
        options.Events.OnAuthorizationCodeReceived = async context =>
        {
            // The 'code_verifier' is an automatically generated value that is used to obtain the auth code. In this
            // case, this value has already been used to retrieve an auth code and must be used again to redeem the
            // server auth code for its access token.
            //
            // See the 'code_verifier' query parameter at these links for further reading.
            //
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-access-token-with-a-client_secret/
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-access-token-with-a-certificate-credential
            context.TokenEndpointRequest.Parameters.TryGetValue("code_verifier", out var codeVerifier);

            if (string.IsNullOrEmpty(codeVerifier))
            {
                throw new Exception("Unable to retrive verify code challenge.");
            }

            // Exchange the auth code for an access token and client-side auth code.
            //
            // The code used in the front-end to exchange for an authentication token is called the 'SpaAuthCode' which
            // is passed along in the 'Microsoft.Identity.Hybrid.Authentication' value.
            //
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.authenticationresult?view=azure-dotnet
            var authResult = await confidentialClientService.GetAuthenticationResultAsync(options.Scope, context.ProtocolMessage.Code, codeVerifier);

            context.Request.HttpContext.Session.SetString("Microsoft.Identity.Hybrid.Authentication", authResult.SpaAuthCode);

            context.HandleCodeRedemption(authResult.AccessToken, authResult.IdToken);
        };

        options.Events.OnRedirectToIdentityProviderForSignOut = async context => {
            var oid = context.HttpContext.User.GetObjectId();
            var tid = context.HttpContext.User.GetTenantId();

            if (!string.IsNullOrEmpty(oid) && !string.IsNullOrEmpty(tid))
            {
                await confidentialClientService.RemoveAccount($"{oid}.{tid}");
            }
        };

        builder.Configuration.Bind("AzureAd", options);
    });

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();
