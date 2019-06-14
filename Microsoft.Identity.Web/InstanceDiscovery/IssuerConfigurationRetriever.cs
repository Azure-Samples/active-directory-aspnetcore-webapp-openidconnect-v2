/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;

namespace Microsoft.Identity.Web.InstanceDiscovery
{
    /// <summary>
    /// An implementation of IConfigurationRetriever geared towards Azure AD issuers metadata />
    /// </summary>
    internal class IssuerConfigurationRetriever : IConfigurationRetriever<IssuerMetadata>
    {
        /// <summary>Retrieves a populated configuration given an address and an <see cref="T:Microsoft.IdentityModel.Protocols.IDocumentRetriever"/>.</summary>
        /// <param name="address">Address of the discovery document.</param>
        /// <param name="retriever">The <see cref="T:Microsoft.IdentityModel.Protocols.IDocumentRetriever"/> to use to read the discovery document.</param>
        /// <param name="cancel">A cancellation token that can be used by other objects or threads to receive notice of cancellation. <see cref="T:System.Threading.CancellationToken"/>.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="address"/> is null or empty.
        /// or
        /// retriever - No metadata document retriever is provided</exception>
        public async Task<IssuerMetadata> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException(nameof(address), $"Azure AD Issuer metadata address url is required");

            if (retriever == null)
                throw new ArgumentNullException(nameof(retriever), $"No metadata document retriever is provided");

            string doc = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<IssuerMetadata>(doc);
        }
    }
}