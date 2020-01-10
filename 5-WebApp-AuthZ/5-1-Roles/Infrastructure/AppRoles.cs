using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    /// <summary>
    /// Contains a list of all the Azure Ad app roles this app works with
    /// </summary>
    public static class AppRoles
    {
        public const string UserReaders = "UserReaders";
        public const string DirectoryViewers = "DirectoryViewers";
    }

    public static class AppPolicies
    {
        public const string UserReadersOnly = "UserReadersOnly";
        public const string DirectoryViewersOnly = "DirectoryViewersOnly";
    }
}
