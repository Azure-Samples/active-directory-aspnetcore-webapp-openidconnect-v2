namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public static class Constants
    {
        public const string ScopeUserRead = "User.Read";
        public const string ScopeUserReadAll = "User.ReadBasic.All";
        public const string BearerAuthorizationScheme = "Bearer";
        public const string UserConsentDeclinedErrorMessage = "User declined to consent to access the app";
        public const string NoUserRole = "The user or administrator has not consented to use the application with ID";
    }
}