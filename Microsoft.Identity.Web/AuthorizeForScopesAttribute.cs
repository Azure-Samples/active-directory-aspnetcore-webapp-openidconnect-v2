// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Filter used on a controller action to trigger incremental consent.
    /// </summary>
    /// <example>
    /// The following controller action will trigger
    /// <code>
    /// [AuthorizeForScopes(Scopes = new[] {"Mail.Send"})]
    /// public async Task&lt;IActionResult&gt; SendEmail()
    /// {
    /// }
    /// </code>
    /// </example>
    public class AuthorizeForScopesAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Scopes to request
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Key section on the configuration file that holds the scope value
        /// </summary>
        public string ScopeKeySection { get; set; }

        /// <summary>
        /// Handles the MsalUiRequiredException
        /// </summary>
        /// <param name="context">Context provided by ASP.NET Core</param>
        public override void OnException(ExceptionContext context)
        {
            // Do not re-use the attribute param Scopes. For more info: https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/issues/273
            string[] incrementalConsentScopes = new string[] { };
            MsalUiRequiredException msalUiRequiredException = context.Exception as MsalUiRequiredException;

            if (msalUiRequiredException == null)
            {
                msalUiRequiredException = context.Exception?.InnerException as MsalUiRequiredException;
            }

            if (msalUiRequiredException != null)
            {
                if (CanBeSolvedByReSignInOfUser(msalUiRequiredException))
                {
                    // the users cannot provide both scopes and ScopeKeySection at the same time
                    if (!string.IsNullOrWhiteSpace(ScopeKeySection) && Scopes != null && Scopes.Length > 0)
                    {
                        throw new InvalidOperationException($"Either provide the '{nameof(ScopeKeySection)}' or the '{nameof(Scopes)}' to the 'AuthorizeForScopes'.");
                    }

                    // If the user wishes us to pick the Scopes from a particular config setting.
                    if (!string.IsNullOrWhiteSpace(ScopeKeySection))
                    {
                        // Load the injected IConfiguration
                        IConfiguration configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                        if (configuration == null)
                        {
                            throw new InvalidOperationException($"The {nameof(ScopeKeySection)} is provided but the IConfiguration instance is not present in the services collection");
                        }

                        incrementalConsentScopes = new string[] { configuration.GetValue<string>(ScopeKeySection) };
                        
                        if (Scopes != null && Scopes.Length > 0 && incrementalConsentScopes != null && incrementalConsentScopes.Length > 0)
                        {
                           throw new InvalidOperationException("no scopes provided in scopes...");
                        }
                    }
                    else
                        incrementalConsentScopes = Scopes;

                    var properties = BuildAuthenticationPropertiesForIncrementalConsent(incrementalConsentScopes, msalUiRequiredException, context.HttpContext);
                    context.Result = new ChallengeResult(properties);
                }
            }

            base.OnException(context);
        }

        private bool CanBeSolvedByReSignInOfUser(MsalUiRequiredException ex)
        {
            // ex.ErrorCode != MsalUiRequiredException.UserNullError indicates a cache problem.
            // When calling an [Authenticate]-decorated controller we expect an authenticated
            // user and therefore its account should be in the cache. However in the case of an
            // InMemoryCache, the cache could be empty if the server was restarted. This is why
            // the null_user exception is thrown.

            return ex.ErrorCode.ContainsAny(new[] { MsalError.UserNullError, MsalError.InvalidGrantError });
        }

        /// <summary>
        /// Build Authentication properties needed for incremental consent.
        /// </summary>
        /// <param name="scopes">Scopes to request</param>
        /// <param name="ex">MsalUiRequiredException instance</param>
        /// <param name="context">current http context in the pipeline</param>
        /// <returns>AuthenticationProperties</returns>
        private AuthenticationProperties BuildAuthenticationPropertiesForIncrementalConsent(
            string[] scopes,
            MsalUiRequiredException ex,
            HttpContext context)
        {
            var properties = new AuthenticationProperties();

            // Set the scopes, including the scopes that ADAL.NET / MSAL.NET need for the token cache
            string[] additionalBuiltInScopes =
                {OidcConstants.ScopeOpenId,
                OidcConstants.ScopeOfflineAccess,
                OidcConstants.ScopeProfile};
            properties.SetParameter<ICollection<string>>(OpenIdConnectParameterNames.Scope,
                                                         scopes.Union(additionalBuiltInScopes).ToList());

            // Attempts to set the login_hint to avoid the logged-in user to be presented with an account selection dialog
            var loginHint = context.User.GetLoginHint();
            if (!string.IsNullOrWhiteSpace(loginHint))
            {
                properties.SetParameter(OpenIdConnectParameterNames.LoginHint, loginHint);

                var domainHint = context.User.GetDomainHint();
                properties.SetParameter(OpenIdConnectParameterNames.DomainHint, domainHint);
            }

            // Additional claims required (for instance MFA)
            if (!string.IsNullOrEmpty(ex.Claims))
            {
                properties.Items.Add(OidcConstants.AdditionalClaims, ex.Claims);
            }

            return properties;
        }
    }
}