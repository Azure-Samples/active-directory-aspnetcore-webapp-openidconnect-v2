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
        static Func<RedirectContext, Task> onRedirectToIdentityProvider;
        //
        // Summary:
        //     Invoked when a protocol message is first received.
        static Func<MessageReceivedContext, Task> onMessageReceived;
        //
        // Summary:
        //     Invoked after security token validation if an authorization code is present in
        //     the protocol message.
        static Func<AuthorizationCodeReceivedContext, Task> onAuthorizationCodeReceived;
        //
        // Summary:
        //     Invoked after "authorization code" is redeemed for tokens at the token endpoint.
        static Func<TokenResponseReceivedContext, Task> onTokenResponseReceived;
        //
        // Summary:
        //     Invoked when an IdToken has been validated and produced an AuthenticationTicket.
        static Func<TokenValidatedContext, Task> onTokenValidated;
        //
        // Summary:
        //     Invoked when user information is retrieved from the UserInfoEndpoint.
        static Func<UserInformationReceivedContext, Task> onUserInformationReceived;
        //
        // Summary:
        //     Invoked if exceptions are thrown during request processing. The exceptions will
        //     be re-thrown after this event unless suppressed.
        static Func<AuthenticationFailedContext, Task> onAuthenticationFailed;
        //
        // Summary:
        //     Invoked when a request is received on the RemoteSignOutPath.
        static Func<RemoteSignOutContext, Task> onRemoteSignOut;
        //
        // Summary:
        //     Invoked before redirecting to the identity provider to sign out.
        static Func<RedirectContext, Task> onRedirectToIdentityProviderForSignOut;
        //
        // Summary:
        //     Invoked before redirecting to the Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions.SignedOutRedirectUri
        //     at the end of a remote sign-out flow.
        static Func<RemoteSignOutContext, Task> onSignedOutCallbackRedirect;
 

        /// <summary>
        /// Subscribes to all the OpenIdConnect events, to help debugging, while
        /// preserving the previous handlers (which are called)
        /// </summary>
        /// <param name="events">Events to subscribe to</param>
        public static void Subscribe(OpenIdConnectEvents events)
        {
            onRedirectToIdentityProvider = events.OnRedirectToIdentityProvider;
            events.OnRedirectToIdentityProvider = OnRedirectToIdentityProvider;

            onMessageReceived = events.OnMessageReceived;
            events.OnMessageReceived = OnMessageReceived;

            onAuthorizationCodeReceived = events.OnAuthorizationCodeReceived;
            events.OnAuthorizationCodeReceived = OnAuthorizationCodeReceived;

            onTokenResponseReceived = events.OnTokenResponseReceived;
            events.OnTokenResponseReceived = OnTokenResponseReceived;

            onTokenValidated = events.OnTokenValidated;
            events.OnTokenValidated = OnTokenValidated;

            onUserInformationReceived = events.OnUserInformationReceived;
            events.OnUserInformationReceived = OnUserInformationReceived;

            onAuthenticationFailed = events.OnAuthenticationFailed;
            events.OnAuthenticationFailed = OnAuthenticationFailed;

            onRemoteSignOut = events.OnRemoteSignOut;
            events.OnRemoteSignOut = OnRemoteSignOut;

            onRedirectToIdentityProviderForSignOut = events.OnRedirectToIdentityProviderForSignOut;
            events.OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut;

            onSignedOutCallbackRedirect = events.OnSignedOutCallbackRedirect;
            events.OnSignedOutCallbackRedirect = OnSignedOutCallbackRedirect;
        }

        static async Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            Debug.WriteLine($"1. Begin {nameof(OnRedirectToIdentityProvider)}");

            await onRedirectToIdentityProvider(context);

            Debug.WriteLine("   Sending OpenIdConnect message:");
            DisplayProtocolMessage(context.ProtocolMessage);
            Debug.WriteLine($"1. End - {nameof(OnRedirectToIdentityProvider)}");
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

        static async Task OnMessageReceived(MessageReceivedContext context)
        {
            Debug.WriteLine($"2. Begin {nameof(OnMessageReceived)}");
            Debug.WriteLine("   Received from STS the OpenIdConnect message:");
            DisplayProtocolMessage(context.ProtocolMessage);
            await onMessageReceived(context);
            Debug.WriteLine($"2. End - {nameof(OnMessageReceived)}");
        }

        static async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            Debug.WriteLine($"4. Begin {nameof(OnAuthorizationCodeReceived)}");
            await onAuthorizationCodeReceived(context);
            Debug.WriteLine($"4. End - {nameof(OnAuthorizationCodeReceived)}");
        }

        static async Task OnTokenResponseReceived(TokenResponseReceivedContext context)
        {
            Debug.WriteLine($"5. Begin {nameof(OnTokenResponseReceived)}");
            await onTokenResponseReceived(context);
            Debug.WriteLine($"5. End - {nameof(OnTokenResponseReceived)}");

        }
        static async Task OnTokenValidated(TokenValidatedContext context)
        {
            Debug.WriteLine($"3. Begin {nameof(OnTokenValidated)}");
            await onTokenValidated(context);
            Debug.WriteLine($"3. End - {nameof(OnTokenValidated)}");
        }
        static async Task OnUserInformationReceived(UserInformationReceivedContext context)
        {
            Debug.WriteLine($"6. Begin {nameof(OnUserInformationReceived)}");
            await onUserInformationReceived(context);
            Debug.WriteLine($"6. End - {nameof(OnUserInformationReceived)}");
        }

        static async Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            Debug.WriteLine($"99. Begin {nameof(OnAuthenticationFailed)}");
            await onAuthenticationFailed(context);
            Debug.WriteLine($"99. End - {nameof(OnAuthenticationFailed)}");
        }

        static async Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            Debug.WriteLine($"10. Begin {nameof(OnRedirectToIdentityProviderForSignOut)}");
            await onRedirectToIdentityProviderForSignOut(context);
            Debug.WriteLine($"10. End - {nameof(OnRedirectToIdentityProviderForSignOut)}");
        }
        static async Task OnRemoteSignOut(RemoteSignOutContext context)
        {
            Debug.WriteLine($"11. Begin {nameof(OnRemoteSignOut)}");
            await onRemoteSignOut(context);
            Debug.WriteLine($"11. End - {nameof(OnRemoteSignOut)}");
        }
        static async Task OnSignedOutCallbackRedirect(RemoteSignOutContext context)
        {
            Debug.WriteLine($"12. Begin {nameof(OnSignedOutCallbackRedirect)}");
            await onSignedOutCallbackRedirect(context);
            Debug.WriteLine($"12. End {nameof(OnSignedOutCallbackRedirect)}");
        }
    }
}
