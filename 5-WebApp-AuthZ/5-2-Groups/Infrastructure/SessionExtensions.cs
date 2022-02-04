using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public static class SessionExtensions
    {
        public static void SetAsByteArray(this ISession session, string key, object toSerialize)
        {
            using MemoryStream memoryStream = new();
            using StreamReader reader = new(memoryStream);
            DataContractSerializer serializer = new(toSerialize.GetType());
            serializer.WriteObject(memoryStream, toSerialize);
            session.Set(key, memoryStream.ToArray());
        }

        public static object GetAsByteArray(this ISession session, string key)
        {
            List<string> stateList = new();
            using (Stream memoryStream = new MemoryStream())
            {
                var objectBytes = session.Get(key);
                memoryStream.Write(objectBytes, 0, objectBytes.Length);
                memoryStream.Position = 0;
                DataContractSerializer deserializer = new(stateList.GetType());
                stateList = (List<string>)deserializer.ReadObject(memoryStream);
            }

            return stateList;
        }
    }
}
