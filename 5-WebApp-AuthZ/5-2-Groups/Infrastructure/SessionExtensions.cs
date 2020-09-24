using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public static class SessionExtensions
    {
        public static void SetAsByteArray(this ISession session, string key, object toSerialize)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, toSerialize);

            session.Set(key, memoryStream.ToArray());
        }

        public static object GetAsByteArray(this ISession session, string key)
        {
            var memoryStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();

            var objectBytes = session.Get(key) as byte[];
            memoryStream.Write(objectBytes, 0, objectBytes.Length);
            memoryStream.Position = 0;

            return binaryFormatter.Deserialize(memoryStream);

        }
    }
}