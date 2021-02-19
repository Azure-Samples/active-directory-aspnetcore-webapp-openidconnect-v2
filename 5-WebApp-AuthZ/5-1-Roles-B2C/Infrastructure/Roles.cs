namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    /// <summary>
    /// Contains a list of all the Azure AD app roles this app depends on and works with.
    /// </summary>
    public static class Role
    {
        /// <summary>
        /// User readers can read basic profiles of all users in the directory.
        /// </summary>
        public const string User = "User";

        /// <summary>
        /// Directory viewers can view objects in the whole directory.
        /// </summary>
        public const string Admin = "Admin";
    }

    /// <summary>
    /// Wrapper class the contain all the authorization policies available in this application.
    /// </summary>
    public static class AuthorizationPolicies
    {
        public const string AssignmentToUserRoleRequired = "AssignmentToUserRoleRequired";
        public const string AssignmentToAdminRoleRequired = "AssignmentToAdminRoleRequired";
    }
}