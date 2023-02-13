## How the code was created

<details>
 <summary>Expand the section</summary>

 The sample is based on [ASP.NET CORE API template](https://learn.microsoft.com/aspnet/core/tutorials/first-web-api?view=aspnetcore-7.0&tabs=visual-studio)
 During the project configuration, specify `Microsoft Identity Platform` inside `Authentication Type` dropdown box. As IDE installs the solution, it might require to install an additional components.
 
 After the initial project was created, we have to continue with further configuration and tweaking:

 1. Replace initial Microsoft Identity Web code

 ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
 ```

 by more detailed configuration which is intended to verify tenant inside token

 ```csharp
  services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(options =>
                        {
                            Configuration.Bind("AzureAd", options);
                            options.Events.OnTokenValidated = async context =>
                            {
                                string tenantId = context.SecurityToken.Claims.FirstOrDefault(x => x.Type == "tid" || x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

                                if (string.IsNullOrWhiteSpace(tenantId))
                                    throw new UnauthorizedAccessException("Unable to get tenantId from token.");

                                var dbContext = context.HttpContext.RequestServices.GetRequiredService<SampleDbContext>();

                                var authorizedTenant = await dbContext.AuthorizedTenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);

                                if (authorizedTenant == null)
                                    throw new UnauthorizedTenantException("This tenant is not authorized");
                            };
                            options.Events.OnAuthenticationFailed = (context) =>
                            {
                                if (context.Exception != null && context.Exception is UnauthorizedTenantException)
                                {
                                    context.Response.Redirect("/Home/UnauthorizedTenant");
                                    context.HandleResponse(); // Suppress the exception
                                }

                                return Task.FromResult(0);
                            };
                        }
                    )
                    .EnableTokenAcquisitionToCallDownstreamApi(options =>                
                        {
                            Configuration.Bind("AzureAd", options);
                        },
                        new string[] { GraphScope.UserReadAll }
                    )
                    .AddInMemoryTokenCaches();

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();
            services.AddRazorPages();
 ```

2. Add SQL server configuration. The database will be used to store ToDo list on local instance of SQL Server. There is an option to use in-memory server as well:

```csharp
 //If you want to run this sample using in memory db, uncomment the line below (options.UseInMemoryDatabase) and comment the one that uses options.UseSqlServer.
 //services.AddDbContext<SampleDbContext>(options => options.UseInMemoryDatabase(databaseName: "MultiTenantOnboarding"));
 services.AddDbContext<SampleDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SampleDbConnStr")));
```

3. Add same site cookies policy:

```csharp
 services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });
```

4. Add a ToDo Service (you will have to create a model/interface as well)

```csharp
 services.AddScoped<ITodoItemService, TodoItemService>();
```

5. Configure MS Graph support

```csharp
 services.AddScoped<IMSGraphService, MSGraphService>();
```

6. Finally configure Authorization Policy and add Razor Pages:
   
```csharp
  services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

  services.AddRazorPages();
```

7. Delete default Controller and Models and create Home, Onboarding and ToDoList controllers
8. Create models for Authorized tenant and ToDo item
9. To efficiently communicate with MSGraph API, create MSGraph Service
10. Create ToDo List Service to work with ToDo items
11. Refer to the [sample](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-3-Multi-Tenant) for more details

 </details>
