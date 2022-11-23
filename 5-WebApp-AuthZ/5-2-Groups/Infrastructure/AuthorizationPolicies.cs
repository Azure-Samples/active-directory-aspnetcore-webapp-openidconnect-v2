namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    /// <summary>
    /// Contain all the authorization policies available in this application.
    /// </summary>
    public static class AuthorizationPolicies
    {
        /// <summary>
        /// this policy stipulates that users in both GroupMember and GroupAdmin can access resources
        /// </summary>
        public const string AssignmentToGroupMemberGroupRequired = "AssignmentToGroupMemberGroupRequired";

        /// <summary>
        /// this policy stipulates that users in GroupAdmin can access resources
        /// </summary>
        public const string AssignmentToGroupAdminGroupRequired = "AssignmentToGroupAdminGroupRequired";
    }
}