using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _2_1_Call_MSGraph.Models
{
    /// <summary>
    /// Helper class to wrap exceptions from insufficient claims challenges
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class WebApiMsalUiRequiredException : Exception
    {
        private HttpResponseMessage httpResponseMessage;
        public WebApiMsalUiRequiredException(string message, HttpResponseMessage response) : base(message)
        {
            httpResponseMessage = response;
        }
        public HttpStatusCode StatusCode
        {
            get { return httpResponseMessage.StatusCode; }
        }

        public HttpResponseHeaders Headers
        {
            get { return httpResponseMessage.Headers; }
        }

        public HttpResponseMessage HttpResponseMessage
        {
            get { return httpResponseMessage; }
        }
    }
}
