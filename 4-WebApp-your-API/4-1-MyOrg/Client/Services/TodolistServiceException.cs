using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TodoListClient.Services
{
    /// <summary>
    /// Specialized excpetion handler for the TodoListService
    /// </summary>
    public class TodolistServiceException : Exception
    {
        private HttpResponseMessage httpResponseMessage;

        public TodolistServiceException(string message, HttpResponseMessage httpResponse) : base(message)
        {
            httpResponseMessage = httpResponse;
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