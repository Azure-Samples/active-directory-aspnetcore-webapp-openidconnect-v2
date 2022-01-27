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
using System.Threading.Tasks;

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
            // Setting configuration for protected web api and extending it to control which tenant will be able to access the API
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            //get list of allowed tenants from configuration
            var allowedTenants = Configuration.GetSection("AzureAd:AllowedTenants").Get<string[]>();

            //configure OnTokenValidated event to filter the tenants
            //you can use either this approach or the one below through policies
            services.Configure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var existingOnTokenValidatedHandler = options.Events.OnTokenValidated;
                    options.Events.OnTokenValidated = async context =>
                    {
                        await existingOnTokenValidatedHandler(context);
                        if (!allowedTenants.Contains(context.Principal.GetTenantId())) 
                        {
                            throw new UnauthorizedAccessException("This tenant is not authorized");
                        }
                    };
                });


            // Creating policies that wraps the authorization requirements
            services.AddAuthorization(

                //uncomment this part if you need to filter the tenants by a policy
                //refer to https://github.com/AzureAD/microsoft-identity-web/wiki/authorization-policies#filtering-tenants

                //builder =>
                //{
                //    string policyName = "User belongs to a specific tenant";
                //    builder.AddPolicy(policyName, b =>
                //    {
                //        b.RequireClaim(ClaimConstants.TenantId, allowedTenants);
                //    });
                //    builder.DefaultPolicy = builder.GetPolicy(policyName);
                //}

            );


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
                //Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
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