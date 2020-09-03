using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Services.GroupProcessing
{
    public class GraphHelper
    {
        /// <summary>
        /// This method inspects the claims collection created from the ID or Access token and detects groups overage. If Groups overage is detected, the method then makes calls to
        /// Microsoft Graph to fetch the group membership of the authenticated user.
        /// </summary>
        /// <param name="context">TokenValidatedContext</param>
        public static async Task ProcessClaimsForGroupsOverage(TokenValidatedContext context)
        {
            try
            {
                // Checks if the incoming token contained a 'Group Overage' claim.
                if (context.Principal.Claims.Any(x => x.Type == "hasgroups" || (x.Type == "_claim_names" && x.Value == "{\"groups\":\"src1\"}")))
                {
                    // For this API call to succeed, the app should have permission 'GroupMember.Read.All' granted.
                    var graph = context.HttpContext.RequestServices.GetService<GraphServiceClient>();

                    if (graph == null)
                    {
                        Console.WriteLine("No service for type 'Microsoft.Graph.GraphServiceClient' has been registered in the Startup.");
                    }
                    else if (context.SecurityToken != null)
                    {
                        // Check if an on-behalf-of all was made to a Web API
                        if (!context.HttpContext.Items.ContainsKey("JwtSecurityTokenUsedToCallWebAPI"))
                        {
                            // extract the cached AT that was presented to the Web API
                            context.HttpContext.Items.Add("JwtSecurityTokenUsedToCallWebAPI", context.SecurityToken as JwtSecurityToken);
                        }

                        // We do not want to pull all attributes of a group from MS Graph, so we use a 'select' to just pick the ones we need.
                        string select = "id,displayName,onPremisesNetBiosName,onPremisesDomainName,onPremisesSamAccountNameonPremisesSecurityIdentifier";

                        // TODO: this line needs a try-catch, with the exception error message being "A call to Microsoft Graph failed, the error is <whatever>"
                        // Make a Graph call to get groups and directory roles that the user is a direct member of.
                        var memberPage = await graph.Me.MemberOf.Request().Select(select).GetAsync().ConfigureAwait(false);

                        if (memberPage?.Count > 0)
                        {
                            // If the result is paginated, this method will process all the pages for us.
                            var allgroups = ProcessIGraphServiceMemberOfCollectionPage(memberPage);

                            if (allgroups?.Count > 0)
                            {
                                var identity = (ClaimsIdentity)context.Principal.Identity;

                                if (identity != null)
                                {
                                    // Remove any existing groups claim
                                    RemoveExistingClaim(identity);

                                    List<Claim> groupClaims = new List<Claim>();

                                    // Re-populate the `groups` claim with the complete list of groups fetched from MS Graph
                                    foreach (Group group in allgroups)
                                    {
                                        // The following code adds group ids to the 'groups' claim. But depending upon your reequirement and the format of the 'groups' claim selected in
                                        // the app registration, you might want to add other attributes than id to the `groups` claim, examples being;

                                        // For instance if the required format is 'NetBIOSDomain\sAMAccountName' then the code is as commented below:
                                        // groupClaims.AddClaim(new Claim("groups", group.OnPremisesNetBiosName+"\\"+group.OnPremisesSamAccountName));
                                        groupClaims.Add(new Claim("groups", group.Id));
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
                if (context.HttpContext.Items.ContainsKey("JwtSecurityTokenUsedToCallWebAPI"))
                {
                    // TODO: The following comment makes no sense !
                    // Remove the key as Microsoft.Identity.Web library utilizes this key.
                    // If not removed then it can cause failure to the application.
                    context.HttpContext.Items.Remove("JwtSecurityTokenUsedToCallWebAPI");
                }
            }
        }

        /// <summary>
        /// Remove groups claims if already exists.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="identity"></param>
        private static void RemoveExistingClaim(ClaimsIdentity identity)
        {
            // clear an existing claim
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