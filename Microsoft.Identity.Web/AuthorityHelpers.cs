using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Identity.Web
{
    public static class AuthorityHelpers
    {
        public static bool IsV2Authority(string authority)
        {
            if (string.IsNullOrEmpty(authority))
                return false;

            return authority.EndsWith("/v2.0");
        }
    }
}
