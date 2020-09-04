using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    public class GraphHelper
    {
        /// <summary>
        /// This method inspects the claims collection created from the ID or Access token issued to a user and returns the groups that are present in the token . If it detects groups overage,
        /// the method then makes calls to Microsoft Graph to fetch the group membership of the authenticated user.
        /// </summary>
        /// <param name="context">TokenValidatedContext</param>
        public static async Task<List<string>> GetSignedInUsersGroups(TokenValidatedContext context)
        {
            List<string> groupClaims = new List<string>();

            try
            {
                // Checks if the incoming token contained a 'Group Overage' claim.
                if (context.Principal.Claims.Any(x => x.Type == "hasgroups" || (x.Type == "_claim_names" && x.Value == "{\"groups\":\"src1\"}")))
                {
                    // Before instatntiating GraphServiceClient, the app should have granted admin consent for 'GroupMember.Read.All' permission.
                    var graphClient = context.HttpContext.RequestServices.GetService<GraphServiceClient>();

                    if (graphClient == null)
                    {
                        Console.WriteLine("No service for type 'Microsoft.Graph.GraphServiceClient' has been registered in the Startup.");
                    }

                    // Checks if the SecurityToken is not null.
                    // For the Web App, SecurityToken contains value of the ID Token.
                    else if (context.SecurityToken != null)
                    {
                        // Checks if 'JwtSecurityTokenUsedToCallWebAPI' key already exists.
                        // This key is required to acquire Access Token for Graph Service Client.
                        if (!context.HttpContext.Items.ContainsKey("JwtSecurityTokenUsedToCallWebAPI"))
                        {
                            // For Web App, access token is retrieved using account identifier. But at this point account identifier is null.
                            // So, SecurityToken is saved in 'JwtSecurityTokenUsedToCallWebAPI' key.
                            // The key is then used to get the Access Token on-behalf of user.
                            context.HttpContext.Items.Add("JwtSecurityTokenUsedToCallWebAPI", context.SecurityToken as JwtSecurityToken);
                        }

                        // The properties that we want to retrieve from MemberOf endpoint.
                        string select = "id,displayName,onPremisesNetBiosName,onPremisesDomainName,onPremisesSamAccountNameonPremisesSecurityIdentifier";

                        IUserMemberOfCollectionWithReferencesPage memberPage = new UserMemberOfCollectionWithReferencesPage();
                        try
                        {
                            //Request to get groups and directory roles that the user is a direct member of.
                            memberPage = await graphClient.Me.MemberOf.Request().Select(select).GetAsync().ConfigureAwait(false);
                        }
                        catch (Exception graphEx)
                        {
                            var exMsg = graphEx.InnerException != null ? graphEx.InnerException.Message : graphEx.Message;
                            Console.WriteLine("Call to Microsoft Graph failed: " + exMsg);
                        }

                        if (memberPage?.Count > 0)
                        {
                            // There is a limit to number of groups returned, below method make calls to Microsoft graph to get all the groups.
                            var allgroups = ProcessIGraphServiceMemberOfCollectionPage(memberPage);

                            if (allgroups?.Count > 0)
                            {
                                var identity = (ClaimsIdentity)context.Principal.Identity;

                                if (identity != null)
                                {
                                    // Remove any existing groups claim
                                    RemoveExistingClaims(identity);

                                    // Re-populate the `groups` claim with the complete list of groups fetched from MS Graph
                                    foreach (Group group in allgroups)
                                    {
                                        // The following code adds group ids to the 'groups' claim. But depending upon your reequirement and the format of the 'groups' claim selected in
                                        // the app registration, you might want to add other attributes than id to the `groups` claim, examples being;

                                        // For instance if the required format is 'NetBIOSDomain\sAMAccountName' then the code is as commented below:
                                        // groupClaims.Add(group.OnPremisesNetBiosName+"\\"+group.OnPremisesSamAccountName));
                                        groupClaims.Add(group.Id);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Checks if the key 'JwtSecurityTokenUsedToCallWebAPI' exists.
                if (context.HttpContext.Items.ContainsKey("JwtSecurityTokenUsedToCallWebAPI"))
                {
                    // Removes 'JwtSecurityTokenUsedToCallWebAPI' from Items collection.
                    // If not removed then it can cause failure to the application.
                    // Because this key is also added by StoreTokenUsedToCallWebAPI method of Microsoft.Identity.Web.
                    context.HttpContext.Items.Remove("JwtSecurityTokenUsedToCallWebAPI");
                }
            }

            // Here we add the groups in a session variable to print on the home page for informational purposes
            context.HttpContext.Session.SetAsByteArray("groupClaims", groupClaims);

            return groupClaims;
        }

        /// <summary>
        /// Remove groups claims if already exists.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="identity"></param>
        private static void RemoveExistingClaims(ClaimsIdentity identity)
        {
            //clear existing claim
            List<Claim> existingGroupsClaims = identity.Claims.Where(x => x.Type == "groups").ToList();
            if (existingGroupsClaims?.Count > 0)
            {
                foreach (Claim groupsClaim in existingGroupsClaims)
                {
                    identity.RemoveClaim(groupsClaim);
                }
            }
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