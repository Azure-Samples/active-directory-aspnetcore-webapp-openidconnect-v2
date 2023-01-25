using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
// By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
// e.g. 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
// This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(builder.Configuration.GetSection("DownstreamApi:Scopes").Value.Split(' '))
    .AddMicrosoftGraph(builder.Configuration.GetValue<string>("DownstreamApi:BaseUrl"), builder.Configuration.GetValue<string>("DownstreamApi:Scopes"))
    .AddInMemoryTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options => {
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.Events = new RejectSessionCookieWhenAccountNotInCacheEvents();

    //options.Events.OnRedirectToLogin = context =>
    //{
    //    context.Response.StatusCode = 401;
    //    return Task.CompletedTask;
    //};

    //options.Events.OnRedirectToAccessDenied = context =>
    //{
    //    context.Response.StatusCode = 401;
    //    return Task.CompletedTask;
    //};
});

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
