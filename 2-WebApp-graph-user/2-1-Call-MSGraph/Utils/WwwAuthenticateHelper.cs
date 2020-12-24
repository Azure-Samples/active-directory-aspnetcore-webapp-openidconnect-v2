using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Net;
using System.Net.Http;
using Microsoft.Graph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebApp_OpenIDConnect_DotNet.Utils
{
    /// <summary>
    /// Helper class that processes the content of a WWW-Authenticate header
    /// Flow details <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/app-resilience-continuous-access-evaluation">How to use Continuous Access Evaluation enabled APIs in your applications</a>
    /// </summary>
    public class WwwAuthenticateHelper
    {
        bool _isBearer = false;
        string _error;
        string _claims;

        public bool isBearer
        {
            get { return _isBearer; }
        }

        public string Error
        {
            get { return _error; }
        }

        public string Claims
        {
            get { return _claims; }
        }

        public static string CreateClaims(string value)
        {
            JObject o = new JObject(
                new JProperty("id_token",
                    new JObject(
                        new JProperty("acrs",
                            new JObject(

                                new JProperty("essential", "true"),
                                new JProperty("value", value)

                           )
                        ))
                    )
                );

            return o.ToString(Formatting.None, null);
        }
        public WwwAuthenticateHelper(HttpHeaderValueCollection<AuthenticationHeaderValue> WwwAuthenticationHeader)
        {
            if (null == WwwAuthenticationHeader)
            {
                return;
            }

            char[] pc = new char[2] { ' ', '\"' };
            string header = WwwAuthenticationHeader.ToString().Trim();


            if (header.StartsWith("Bearer", System.StringComparison.InvariantCultureIgnoreCase))
            {
                _isBearer = true;
                header = header.Remove(0, "Bearer".Length);
            }

            string[] parameters = header.Split(',');
            if (parameters.Length > 0)
            {
                foreach (string parameter in parameters)
                {
                    int i = parameter.IndexOf('=');

                    if (i > 0 && i < (parameter.Length - 1))
                    {
                        string key = parameter.Substring(0, i).Trim(pc).ToLower();
                        string value = parameter.Substring(i + 1).Trim(pc);

                        switch (key)
                        {
                            case "claims":
                                var base64EncodedBytes = System.Convert.FromBase64String(value);
                                _claims = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                                break;
                            case "error":
                                _error = value;
                                break;
                        }
                    }
                }
            }

        }
    }
}
