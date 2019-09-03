// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Identity.Web.TokenCacheProviders.Sql
{
    /// <summary>
    /// MSAL's Sql token cache options
    /// </summary>
    public class MsalSqlTokenCacheOptions
    {
        /// <summary>Initializes a new instance of the <see cref="MsalSqlTokenCacheOptions"/> class.</summary>
        /// <param name="sqlConnectionString">the SQL connection string to the token cache database.</param>
        public MsalSqlTokenCacheOptions(string sqlConnectionString) :
            this(sqlConnectionString, string.Empty)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MsalSqlTokenCacheOptions"/> class.</summary>
        /// <param name="sqlConnectionString">The SQL connection string.</param>
        /// <param name="clientId">The the clientId of the application for whom this token cache instance is being created. (Optional for User cache).</param>
        public MsalSqlTokenCacheOptions(string sqlConnectionString, string clientId)
        {
            SqlConnectionString = sqlConnectionString;
            ClientId = clientId;
        }

        /// <summary>
        /// Gets or sets the SQL connection string to the token cache database.
        /// </summary>
        public string SqlConnectionString { get; }

        /// <summary>
        /// Gets or sets the clientId of the application for whom this token cache instance is being created. (Optional)
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Application name for the Data protection. 
        /// To share protected payloads among apps, configure SetApplicationName in each app with the same value. 
        /// </summary>
        public string ApplicationName { get; set; } = "WebApp_Tutorial";
    }
}