using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ToDoListService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;
using System;

namespace ToDoListService
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
            // Setting configuration for protected web api

            services.AddMicrosoftIdentityWebApiAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddInMemoryTokenCaches();

            // Comment above lines of code and uncomment this section if you would like to validate ID tokens for allowed tenantIds
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //  .AddMicrosoftIdentityWebApi(options =>
            //{
            //    Configuration.Bind("AzureAd", options);
            //    options.Events = new JwtBearerEvents();
            //    options.Events.OnTokenValidated = async context =>
            //    {
            //        string[] allowedTenants = {/* list of tenant IDs */ };
            //        string tenantId = context.Principal.Claims.FirstOrDefault(x => x.Type == "tid" || x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            //        if (!allowedTenants.Contains(tenantId))
            //        {
            //            throw new Exception("This tenant is not authorized");
            //        }
            //    };
            //}, options => { Configuration.Bind("AzureAd", options); })
            //  .EnableTokenAcquisitionToCallDownstreamApi(
            //        options =>
            //        {
            //             Configuration.Bind("AzureAd", options);
            //        })
            //    .AddInMemoryTokenCaches();

            // Creating policies that wraps the authorization requirements
            services.AddAuthorization();

            services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));

            services.AddControllers();
            
            // Allowing CORS for all domains and methods for the purpose of sample
            //services.AddCors(o => o.AddPolicy("default", builder =>
            //{
            //    builder.AllowAnyOrigin()
            //           .AllowAnyMethod()
            //           .AllowAnyHeader();
            //}));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // Personal Identifiable Information is not written to the logs by default, to be compliant with GDPR.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseCors("default");
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