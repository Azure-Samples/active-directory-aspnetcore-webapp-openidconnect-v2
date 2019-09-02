// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.TokenCacheProviders
{
    /// <summary>
    /// MSAL token cache provider interface for application token cache
    /// </summary>
    public interface IMsalAppTokenCacheProvider : IMsalTokenCacheProvider
    {
        Task InitializeAsync(ITokenCache tokenCache);
    }
}