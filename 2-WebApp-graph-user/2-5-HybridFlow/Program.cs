using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using WebApp_OpenIDConnect_DotNet.Services;

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Add(serviceCollection.Single());

builder.Services.Concat(serviceCollection);

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.ResponseType = "code";
        options.Events.OnAuthorizationCodeReceived = async context =>
        {
            context.TokenEndpointRequest.Parameters.TryGetValue("code_verifier", out var codeVerifier);
            context.HandleCodeRedemption();

            var clientService = serviceProvider.GetService<IConfidentialClientApplicationService>();

            var authResult = await clientService.GetAuthenticationResultAsync(context.ProtocolMessage.Code, codeVerifier);
            var spaCode = authResult.SpaAuthCode;
            context.Response.Cookies.Append("Microsoft.Identity.Hybrid.Authentication.Cookie", spaCode);

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
