// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.Identity.Web.Resource
{
    /// <summary>
    /// Diagnostics used in the Open Id Connect middleware
    /// (used in Web Apps)
    /// </summary>
    public class OpenIdConnectMiddlewareDiagnostics
    {
        //
        // Summary:
        //     Invoked before redirecting to the identity provider to authenticate. This can
        //     be used to set ProtocolMessage.State that will be persisted through the authentication
        //     process. The ProtocolMessage can also be used to add or customize parameters
        //     sent to the identity provider.
        private static Func<RedirectContext, Task> s_onRedirectToIdentityProvider;

        //
        // Summary:
        //     Invoked when a protocol message is first received.
        private static Func<MessageReceivedContext, Task> s_onMessageReceived;

        //
        // Summary:
        //     Invoked after security token validation if an authorization code is present in
        //     the protocol message.
        private static Func<AuthorizationCodeReceivedContext, Task> s_onAuthorizationCodeReceived;

        //
        // Summary:
        //     Invoked after "authorization code" is redeemed for tokens at the token endpoint.
        private static Func<TokenResponseReceivedContext, Task> s_onTokenResponseReceived;

        //
        // Summary:
        //     Invoked when an IdToken has been validated and produced an AuthenticationTicket.
        private static Func<TokenValidatedContext, Task> s_onTokenValidated;

        //
        // Summary:
        //     Invoked when user information is retrieved from the UserInfoEndpoint.
        private static Func<UserInformationReceivedContext, Task> s_onUserInformationReceived;

        //
        // Summary:
        //     Invoked if exceptions are thrown during request processing. The exceptions will
        //     be re-thrown after this event unless suppressed.
        private static Func<AuthenticationFailedContext, Task> s_onAuthenticationFailed;

        //
        // Summary:
        //     Invoked when a request is received on the RemoteSignOutPath.
        private static Func<RemoteSignOutContext, Task> s_onRemoteSignOut;

        //
        // Summary:
        //     Invoked before redirecting to the identity provider to sign out.
        private static Func<RedirectContext, Task> s_onRedirectToIdentityProviderForSignOut;

        //
        // Summary:
        //     Invoked before redirecting to the Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions.SignedOutRedirectUri
        //     at the end of a remote sign-out flow.
        private static Func<RemoteSignOutContext, Task> s_onSignedOutCallbackRedirect;


        /// <summary>
        /// Subscribes to all the OpenIdConnect events, to help debugging, while
        /// preserving the previous handlers (which are called)
        /// </summary>
        /// <param name="events">Events to subscribe to</param>
        public static void Subscribe(OpenIdConnectEvents events)
        {
            s_onRedirectToIdentityProvider = events.OnRedirectToIdentityProvider;
            events.OnRedirectToIdentityProvider = OnRedirectToIdentityProviderAsync;

            s_onMessageReceived = events.OnMessageReceived;
            events.OnMessageReceived = OnMessageReceivedAsync;

            s_onAuthorizationCodeReceived = events.OnAuthorizationCodeReceived;
            events.OnAuthorizationCodeReceived = OnAuthorizationCodeReceivedAsync;

            s_onTokenResponseReceived = events.OnTokenResponseReceived;
            events.OnTokenResponseReceived = OnTokenResponseReceivedAsync;

            s_onTokenValidated = events.OnTokenValidated;
            events.OnTokenValidated = OnTokenValidatedAsync;

            s_onUserInformationReceived = events.OnUserInformationReceived;
            events.OnUserInformationReceived = OnUserInformationReceivedAsync;

            s_onAuthenticationFailed = events.OnAuthenticationFailed;
            events.OnAuthenticationFailed = OnAuthenticationFailedAsync;

            s_onRemoteSignOut = events.OnRemoteSignOut;
            events.OnRemoteSignOut = OnRemoteSignOutAsync;

            s_onRedirectToIdentityProviderForSignOut = events.OnRedirectToIdentityProviderForSignOut;
            events.OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOutAsync;

            s_onSignedOutCallbackRedirect = events.OnSignedOutCallbackRedirect;
            events.OnSignedOutCallbackRedirect = OnSignedOutCallbackRedirectAsync;
        }

        private static async Task OnRedirectToIdentityProviderAsync(RedirectContext context)
        {
            Debug.WriteLine($"1. Begin {nameof(OnRedirectToIdentityProviderAsync)}");

            await s_onRedirectToIdentityProvider(context).ConfigureAwait(false);

            Debug.WriteLine("   Sending OpenIdConnect message:");
            DisplayProtocolMessage(context.ProtocolMessage);
            Debug.WriteLine($"1. End - {nameof(OnRedirectToIdentityProviderAsync)}");
        }

        private static void DisplayProtocolMessage(OpenIdConnectMessage message)
        {
            foreach (var property in message.GetType().GetProperties())
            {
                object value = property.GetValue(message);
                if (value != null)
                {
                    Debug.WriteLine($"   - {property.Name}={value}");
                }
            }
        }

        private static async Task OnMessageReceivedAsync(MessageReceivedContext context)
        {
            Debug.WriteLine($"2. Begin {nameof(OnMessageReceivedAsync)}");
            Debug.WriteLine("   Received from STS the OpenIdConnect message:");
            DisplayProtocolMessage(context.ProtocolMessage);
            await s_onMessageReceived(context).ConfigureAwait(false);
            Debug.WriteLine($"2. End - {nameof(OnMessageReceivedAsync)}");
        }

        private static async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedContext context)
        {
            Debug.WriteLine($"4. Begin {nameof(OnAuthorizationCodeReceivedAsync)}");
            await s_onAuthorizationCodeReceived(context).ConfigureAwait(false);
            Debug.WriteLine($"4. End - {nameof(OnAuthorizationCodeReceivedAsync)}");
        }

        private static async Task OnTokenResponseReceivedAsync(TokenResponseReceivedContext context)
        {
            Debug.WriteLine($"5. Begin {nameof(OnTokenResponseReceivedAsync)}");
            await s_onTokenResponseReceived(context).ConfigureAwait(false);
            Debug.WriteLine($"5. End - {nameof(OnTokenResponseReceivedAsync)}");

        }

        private static async Task OnTokenValidatedAsync(TokenValidatedContext context)
        {
            Debug.WriteLine($"3. Begin {nameof(OnTokenValidatedAsync)}");
            await s_onTokenValidated(context).ConfigureAwait(false);
            Debug.WriteLine($"3. End - {nameof(OnTokenValidatedAsync)}");
        }

        private static async Task OnUserInformationReceivedAsync(UserInformationReceivedContext context)
        {
            Debug.WriteLine($"6. Begin {nameof(OnUserInformationReceivedAsync)}");
            await s_onUserInformationReceived(context).ConfigureAwait(false);
            Debug.WriteLine($"6. End - {nameof(OnUserInformationReceivedAsync)}");
        }

        private static async Task OnAuthenticationFailedAsync(AuthenticationFailedContext context)
        {
            Debug.WriteLine($"99. Begin {nameof(OnAuthenticationFailedAsync)}");
            await s_onAuthenticationFailed(context).ConfigureAwait(false);
            Debug.WriteLine($"99. End - {nameof(OnAuthenticationFailedAsync)}");
        }

        private static async Task OnRedirectToIdentityProviderForSignOutAsync(RedirectContext context)
        {
            Debug.WriteLine($"10. Begin {nameof(OnRedirectToIdentityProviderForSignOutAsync)}");
            await s_onRedirectToIdentityProviderForSignOut(context).ConfigureAwait(false);
            Debug.WriteLine($"10. End - {nameof(OnRedirectToIdentityProviderForSignOutAsync)}");
        }

        private static async Task OnRemoteSignOutAsync(RemoteSignOutContext context)
        {
            Debug.WriteLine($"11. Begin {nameof(OnRemoteSignOutAsync)}");
            await s_onRemoteSignOut(context).ConfigureAwait(false);
            Debug.WriteLine($"11. End - {nameof(OnRemoteSignOutAsync)}");
        }

        private static async Task OnSignedOutCallbackRedirectAsync(RemoteSignOutContext context)
        {
            Debug.WriteLine($"12. Begin {nameof(OnSignedOutCallbackRedirectAsync)}");
            await s_onSignedOutCallbackRedirect(context).ConfigureAwait(false);
            Debug.WriteLine($"12. End {nameof(OnSignedOutCallbackRedirectAsync)}");
        }
    }
}
