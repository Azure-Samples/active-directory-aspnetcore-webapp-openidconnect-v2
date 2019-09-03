// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by Sql server and Entity Framework
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MsalAppSqlTokenCacheProvider : MsalSqlTokenCacheProvider,  IMsalAppTokenCacheProvider
    {
        /// <summary>Initializes a new instance of the <see cref="MsalAppSqlTokenCacheProvider"/> class.</summary>
        /// <param name="tokenCacheDbContext">The token cache database context.</param>
        /// <param name="azureAdOptionsAccessor">The azure ad options accessor.</param>
        /// <param name="protectionProvider">The protection provider.</param>
        /// <exception cref="ArgumentNullException">
        /// protectionProvider - The app token cache needs an {nameof(IDataProtectionProvider)} to operate. Please use 'serviceCollection.AddDataProtection();' to add the data protection provider to the service collection
        /// or
        /// protectionProvider - The app token cache needs the '{nameof(AzureADOptions)}' section in configuration, populated with clientId to initialize.
        /// </exception>
        public MsalAppSqlTokenCacheProvider(IHttpContextAccessor httpContextAccessor, TokenCacheDbContext tokenCacheDbContext, IOptions<AzureADOptions> azureAdOptionsAccessor, IDataProtectionProvider protectionProvider)
            : base(httpContextAccessor, tokenCacheDbContext, azureAdOptionsAccessor, protectionProvider)
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