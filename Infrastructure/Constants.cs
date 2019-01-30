namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public static class Constants
    {
        public const string AdditionalClaims = "claims";
        public const string OpenIdResponseType = "id_token code";
        public const string ScopeOfflineAccess = "offline_access";
        public const string ScopeUserRead = "User.Read";
        public const string ScopeProfile = "profile";
        public const string ScopeOpenId = "openid";
        public const string AuthenticationHeaderValue = "Bearer";
    }
}