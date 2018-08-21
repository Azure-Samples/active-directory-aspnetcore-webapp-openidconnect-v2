using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;
using TodoListService.Extensions;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(_ => { });

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect();
            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;
            private readonly ITokenAcquisition _tokenAcquisition;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions, ITokenAcquisition tokenAcquisition)
            {
                _azureOptions = azureOptions.Value;
                _tokenAcquisition = tokenAcquisition;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                options.ClientId = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}/v2.0";   // V2 specific
                options.UseTokenLifetime = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateIssuer = false;     // accept several tenants
                options.Events = new OpenIdConnectEvents();
                options.Events.OnAuthorizationCodeReceived = OnAuthorizationCodeReceived;
                options.Scope.Add("user.read");

                options.ResponseType = "code id_token";
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            {
                string[] scopes = new string[] { "user.read" };
                await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, scopes);
            }
        }
    }
}
