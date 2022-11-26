using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            return await Task.Run(() =>
            {
                return ProcessWithCAE<User>(
                    async () =>
                    {
                        // Call /me
                        return await _graphServiceClient.Me.Request().GetAsync();
                    });
            });
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetMyPhotoAsync()
        {
            return await Task.Run(() =>
            {
                return ProcessWithCAE<Stream>(
                    async () =>
                    {
                        // Call /me/Photo Api
                        return await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
                    });
            });
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Group>> GetMemberOfAsync()
        {
            return await Task.Run(() =>
            {
                return ProcessWithCAE<IEnumerable<Group>>(
                    async () =>
                    {
                        return ProcessIGraphServiceMemberOfCollectionPage(await _graphServiceClient.Me.MemberOf.Request().GetAsync());
                    });
            });
        }

        /// <summary>
        /// Calls the MS Graph /me/photo endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {

            return await Task.Run(() =>
            {
                return ProcessWithCAE<IEnumerable<User>>(
                    async () =>
                    {
                        return await CollectionProcessor<User>.ProcessGraphCollectionPageAsync(_graphServiceClient, await _graphServiceClient.Users.Request().GetAsync(), 50);
                    });
            });
        }

        private T ProcessWithCAE<T>(Func<Task<T>> processor)
        {
            try
            {
                return processor().Result;
            }

            catch (AggregateException aex)
            {
                if (aex.InnerException is ServiceException exception && aex.InnerException.Message.Contains("Continuous access evaluation resulted in claims challenge"))
                {
                    try
                    {
                        // Get challenge from response of Graph API
                        var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(exception.ResponseHeaders);

                        _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
                    }
                    catch (Exception ex2)
                    {
                        _consentHandler.HandleException(ex2);
                    }
                }

                else if (aex.InnerException is ServiceException photoException && photoException.Error.Code == "ImageNotFound")
                {
                    return default;
                }

                else
                {
                    throw new Exception($"Unknown error just occured. Message: {aex.InnerException.Message}");
                }

                return default;
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