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
        options.ResponseType = OpenIdConnectResponseType.Code;

        // Scopes need to be added in to get proper claims for user.
        var apiScopes = builder.Configuration.GetSection("DownstreamApi:Scopes").Value;

        foreach (var scope in apiScopes.Split(' '))
        {
            options.Scope.Add(scope);
        }

        options.Events.OnAuthorizationCodeReceived = async context =>
        {
            context.TokenEndpointRequest.Parameters.TryGetValue("code_verifier", out var codeVerifier);

            if (string.IsNullOrEmpty(codeVerifier))
            {
                throw new Exception("Unable to retrive verify code challenge.");
            }

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
