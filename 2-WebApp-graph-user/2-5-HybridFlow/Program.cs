using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Services;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp_OpenIDConnect_DotNet.Options;
using Microsoft.Extensions.Options;

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
            context.HandleCodeRedemption();

            if (string.IsNullOrEmpty(codeVerifier))
            {
                throw new Exception("Unable to retrive verify code challenge.");
            }

            var authResult = await confidentialClientService.GetAuthenticationResultAsync(options.Scope, context.ProtocolMessage.Code, codeVerifier);

            context.Request.HttpContext.Session.SetString("Microsoft.Identity.Hybrid.Authentication", authResult.SpaAuthCode);

            context.TokenEndpointResponse.AccessToken = authResult.AccessToken;
            context.TokenEndpointResponse.IdToken = authResult.IdToken;
        };

        builder.Configuration.Bind("AzureAd", options);
    });

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddRazorPages();
builder.Services.AddControllers();

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
app.UseStaticFiles("");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
