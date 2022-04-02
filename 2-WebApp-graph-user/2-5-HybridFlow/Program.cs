using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Services;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp_OpenIDConnect_DotNet.Options;

var builder = WebApplication.CreateBuilder(args);

var serviceCollection = new ServiceCollection();

serviceCollection.Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"));
serviceCollection.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();

var serviceProvider = serviceCollection.BuildServiceProvider();

builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddSession();
builder.Services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.ResponseType = OpenIdConnectResponseType.Code;

        // Scopes need to be added in to get proper claims for user.
        var apiScopes = builder.Configuration.GetSection("DownstreamApi:Scopes").Value;

        foreach(var scope in apiScopes.Split(' ')) {
            options.Scope.Add(scope);
        }

        options.Events.OnAuthorizationCodeReceived = async context =>
        {
            context.TokenEndpointRequest.Parameters.TryGetValue("code_verifier", out var codeVerifier);
            context.HandleCodeRedemption();

            var clientService = serviceProvider.GetService<IConfidentialClientApplicationService>();
            var authResult = await clientService.GetAuthenticationResultAsync(options.Scope, context.ProtocolMessage.Code, codeVerifier);

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
