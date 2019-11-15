using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Utils
{
    public class UnauthorizedTenantException : UnauthorizedAccessException
    {
        public UnauthorizedTenantException():base() { }
        public UnauthorizedTenantException(string message):base(message) { }
    }
}
