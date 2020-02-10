// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Identity.Web
{
    [DataContract]
    internal class ClientInfo
    {
        [DataMember(Name = "uid", IsRequired = false)]
        public string UniqueObjectIdentifier { get; set; }

        [DataMember(Name = "utid", IsRequired = false)]
        public string UniqueTenantIdentifier { get; set; }

        public static ClientInfo CreateFromJson(string clientInfo)
        {
            if (string.IsNullOrEmpty(clientInfo))
            {
                throw new ArgumentNullException(nameof(clientInfo), $"client info returned from the server is null");
            }

            try
            {
                return DeserializeFromJson<ClientInfo>(Base64UrlHelpers.DecodeToBytes(clientInfo));
            }
            catch (Exception exc)
            {
                throw new ArgumentException($"Failed to parse the returned client info. ", nameof(clientInfo));
            }
        }

        internal static T DeserializeFromJson<T>(byte[] jsonByteArray)
        {
            if (jsonByteArray == null || jsonByteArray.Length == 0)
            {
                return default;
            }

            using (var stream = new MemoryStream(jsonByteArray))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return (T)JsonSerializer.Create().Deserialize(reader, typeof(T));
        }
    }
}
