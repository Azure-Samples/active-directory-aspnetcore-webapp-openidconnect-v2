// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAppCallsMicrosoftGraph
{
    internal class TokenAcquisitionCredentialProvider : IAuthenticationProvider
    {
        public TokenAcquisitionCredentialProvider(ITokenAcquisition tokenAcquisition, IEnumerable<string> initialScopes)
        {
            _tokenAcquisition = tokenAcquisition;
            _initialScopes = initialScopes;
        }

        ITokenAcquisition _tokenAcquisition;
        IEnumerable<string> _initialScopes;

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
                request.Headers.Add("Authorization",
               $"Bearer {await _tokenAcquisition.GetAccessTokenForUserAsync(_initialScopes)}");
        }
    }
}
