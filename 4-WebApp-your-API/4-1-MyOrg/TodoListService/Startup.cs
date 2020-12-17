// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System;
using Constants = TodoListService.Models.Constants;

namespace TodoListService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                        .AddInMemoryTokenCaches();

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var existingOnTokenValidatedHandler = options.Events.OnTokenValidated;
                options.Events.OnTokenValidated = async context =>
                {
                    // Let the standard token validation complete
                    await existingOnTokenValidatedHandler(context);

                    // Your code to conduct extra processing will come below

                    // This check is required to ensure that the web API only accepts tokens from tenants where it has been consented and provisioned.
                    if (!context.Principal.Claims.Any(x => x.Type == ClaimConstants.Scope)
                    && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Scp)
                    && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Roles))
                    {
                        throw new UnauthorizedAccessException("Neither scope or roles claim was found in the bearer token.");
                    }

                    // Here we are testimg that the Access token's 'scope' claim has a value of 'access_as_user'. Checking here helps us remove 'AuthorizeForScopes' attribute in the individual API controllers
                    string scope = context.Principal.Claims.FirstOrDefault(x => x.Type == "scp")?.Value;

                    if (scope != Constants.scopeRequiredByApi)
                    {
                        throw new UnauthorizedAccessException("The expected scope was not found in the ones presented by the Access Token.");
                    }
                };
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // PII hiding in log files is enabled by default for GDPR concerns.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}