// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders.Session
{
    /// <summary>
    /// This is a MSAL's TokenCache implementation for one user. It uses Http session as a persistence store
    /// </summary>
    /// For this session cache to work effectively the aspnetcore session has to be configured properly.
    /// The latest guidance is provided at https://docs.microsoft.com/aspnet/core/fundamentals/app-state
    ///
    /// // In the method - public void ConfigureServices(IServiceCollection services) in startup.cs, add the following
    /// services.AddSession(option =>
    /// {
    ///	    option.Cookie.IsEssential = true;
    /// });
    ///
    /// In the method - public void Configure(IApplicationBuilder app, IHostingEnvironment env) in startup.cs, add the following
    ///
    /// app.UseSession(); // Before UseMvc()
    ///
    public class MsalPerUserSessionTokenCacheProvider : MsalSessionTokenCacheProvider, IMsalUserTokenCacheProvider
    {
        public MsalPerUserSessionTokenCacheProvider(IOptions<AzureADOptions> azureAdOptions,
                                   IHttpContextAccessor httpContextAccessor) :
             base(azureAdOptions, httpContextAccessor)
        {

        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        public async Task InitializeAsync(ITokenCache tokenCache)
        {
            await InitializeAsync(tokenCache, true).ConfigureAwait(false);
        }
    }
}