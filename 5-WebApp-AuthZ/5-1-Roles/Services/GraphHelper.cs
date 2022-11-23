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
            User graphUser = null;

            try
            {
                // Call /me Api
                graphUser = await _graphServiceClient.Me.Request().GetAsync();
            }
            catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge")) // CAE challenge occurred
            {
                try
                {
                    // Get challenge from response of Graph API
                    var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);

                    _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
                }
                catch (Exception ex2)
                {
                    _consentHandler.HandleException(ex2);
                }
            }

            return graphUser;
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetMyPhotoAsync()
        {
            try
            {
                // Call /me/Photo Api
                return await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                // Call the /me endpoint of Graph again with a fresh token
                return await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            IGraphServiceUsersCollectionPage userspage = null;

            try
            {
                // Call /users Api
                userspage = await _graphServiceClient.Users.Request().GetAsync();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                // Call the /me endpoint of Graph again with a fresh token
                userspage = await _graphServiceClient.Users.Request().GetAsync();
            }
            return await CollectionProcessor<User>.ProcessGraphCollectionPageAsync(_graphServiceClient, userspage, 50);
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Group>> GetMemberOfAsync()
        {
            IUserMemberOfCollectionWithReferencesPage mymemberships = null;

            try
            {
                // Call /users Api
                mymemberships = await _graphServiceClient.Me.MemberOf.Request().GetAsync();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                // Call the /me endpoint of Graph again with a fresh token
                mymemberships = await _graphServiceClient.Me.MemberOf.Request().GetAsync();
            }
            return ProcessIGraphServiceMemberOfCollectionPage(mymemberships);
        }

        /// <summary>
        /// Returns all the groups that the user is a direct member of.
        /// </summary>
        /// <param name="membersCollectionPage">First page having collection of directory roles and groups</param>
        /// <returns>List of groups</returns>
        private static List<Group> ProcessIGraphServiceMemberOfCollectionPage(IUserMemberOfCollectionWithReferencesPage membersCollectionPage)
        {
            List<Group> allGroups = new List<Group>();

            try
            {
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
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"We could not process the groups list: {ex}");
                return null;
            }
            return allGroups;
        }
    }
}