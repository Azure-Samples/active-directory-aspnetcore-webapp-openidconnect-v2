using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyMiddleware
{
    public class MiddlewareSimple
    {
        private RequestDelegate _next;

        public MiddlewareSimple(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Debug.WriteLine("Incoming request!");
            await _next.Invoke(context);
            Debug.WriteLine("Outgoing response!");
        }
    }

    public class MyMiddlewareWrapper
    {
        public void Configure(IApplicationBuilder builder)
        {
            builder.UseMiddleware<MiddlewareSimple>();
        }
    }
}
