using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class GraphHelper
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly HttpContext _httpContext;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private string[] _graphScopes;
        private readonly int _grapCollectionMaxRows = 50;

        public GraphHelper(HttpContext httpContext, string[] graphScopes)
        {
            this._httpContext = httpContext;

            var services = this._httpContext.RequestServices;

            this._graphServiceClient = (GraphServiceClient)services.GetService(typeof(GraphServiceClient));
            if (this._graphServiceClient == null) throw new NullReferenceException("The GraphServiceClient has not been added to the services collection during the ConfigureServices()");

            this._consentHandler = (MicrosoftIdentityConsentAndConditionalAccessHandler)services.GetService(typeof(MicrosoftIdentityConsentAndConditionalAccessHandler));
            if (this._consentHandler == null) throw new NullReferenceException("The MicrosoftIdentityConsentAndConditionalAccessHandler has not been added to the services collection during the ConfigureServices()");

            this._graphScopes = graphScopes;
        }

        /// <summary>
        /// Calls the MS Graph /me endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<User> GetMeAsync()
        {
            return await CallGraphWithCAEFallback<User>(
                async () =>
                {
                    // Call /me
                    return await _graphServiceClient.Me.Request().GetAsync();
                });
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetMyPhotoAsync()
        {
            try
            {
                return await
                    CallGraphWithCAEFallback<Stream>(
                        async () =>
                        {
                            // Call /me/Photo Api
                            return await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
                        });
            }

            catch (ServiceException svcex) when (svcex.Error.Code == "ImageNotFound")
            {
                return default;
            }
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Group>> GetMemberOfAsync()
        {
            return ProcessIGraphServiceMemberOfCollectionPage(
                await CallGraphWithCAEFallback<IUserMemberOfCollectionWithReferencesPage>(
                    async () =>
                    {
                        return await _graphServiceClient.Me.MemberOf.Request().GetAsync();
                    })
                );
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await CollectionProcessor<User>.ProcessGraphCollectionPageAsync(
                _graphServiceClient

                , await CallGraphWithCAEFallback<ICollectionPage<User>>(
                    async () =>
                    {
                        return await _graphServiceClient.Users.Request().GetAsync();
                    })

                , _grapCollectionMaxRows
            );
        }

        private async Task<T> CallGraphWithCAEFallback<T>(Func<Task<T>> graphApiCaller)
        {
            try
            {
                return await graphApiCaller();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                try
                {
                    // Get challenge from response of Graph API
                    var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(ex.ResponseHeaders);

                    _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
                }
                catch (Exception ex2)
                {
                    _consentHandler.HandleException(ex2);
                }

                return default;
            }

            //in case there is unknown exception which is not Image not found
            catch (Exception ex) when (!ex.Message.Contains("ImageNotFound"))
            {
                throw new Exception($"Unknown error just occured. Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns all the groups that the user is a direct member of.
        /// </summary>
        /// <param name="membersCollectionPage">First page having collection of directory roles and groups</param>
        /// <returns>List of groups</returns>
        private static List<Group> ProcessIGraphServiceMemberOfCollectionPage(IUserMemberOfCollectionWithReferencesPage membersCollectionPage)
        {
            try
            {
                List<Group> allGroups = new List<Group>();

                if (membersCollectionPage != null)
                {
                    do
                    {
                        // Page through results
                        foreach (DirectoryObject directoryObject in membersCollectionPage.CurrentPage)
                        {
                            //Collection contains directory roles and groups of the user.
                            //Checks and adds groups only to the list.
                            if (directoryObject is Group)
                            {
                                allGroups.Add(directoryObject as Group);
                            }
                        }

                        // are there more pages (Has a @odata.nextLink ?)
                        if (membersCollectionPage.NextPageRequest != null)
                        {
                            membersCollectionPage = membersCollectionPage.NextPageRequest.GetAsync().Result;
                        }
                        else
                        {
                            membersCollectionPage = null;
                        }
                    } while (membersCollectionPage != null);
                }

                return allGroups;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"We could not process the groups list: {ex}");
                return null;
            }
        }
    }
}