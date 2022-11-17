using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    /// <summary>
    /// Contain all the authorization policies available in this application.
    /// </summary>
    public static class AuthorizationPolicies
    {
        public const string AssignmentToGroupMemberGroupRequired = "AssignmentToGroupMemberGroupRequired";
        public const string AssignmentToGroupAdminGroupRequired = "AssignmentToGroupAdminGroupRequired";
    }
}