using System.Threading.Tasks;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;

namespace WebApp_OpenIDConnect_DotNet.Services.GroupProcessing
{
    public class GraphHelper
    {
        /// <summary>
        /// Adds groups claim for group overage
        /// </summary>
        /// <param name="context">TokenValidatedContext</param>
        public static async Task ProcessGroupsClaimforAccessToken(TokenValidatedContext context)
        {
            try
            {
                //Checks if the token contains 'Group Overage' Claim.
                if (context.Principal.Claims.Any(x => x.Type == "hasgroups" || (x.Type == "_claim_names" && x.Value == "{\"groups\":\"src1\"}")))
                {
                    //This API should have permission set for Microsoft graph: 'GroupMember.Read.All'
                    var graph = context.HttpContext.RequestServices.GetService<GraphServiceClient>();

                    if (graph == null)
                    {
                        Console.WriteLine("No service for type 'Microsoft.Graph.GraphServiceClient' has been registered.");
                    }
                    else if (context.SecurityToken != null)
                    {
                        if (!context.HttpContext.Items.ContainsKey("JwtSecurityTokenUsedToCallWebAPI"))
                        {
                            //Added current access token in below key to get Access Token on-behalf of user. 
                            context.HttpContext.Items.Add("JwtSecurityTokenUsedToCallWebAPI", context.SecurityToken as JwtSecurityToken);
                        }
                        //Specify the property names in the 'select' variable to get values for the specified properties.
                        string select = "id,displayName,onPremisesNetBiosName,onPremisesDomainName,onPremisesSamAccountNameonPremisesSecurityIdentifier";

                        //Request to get groups and directory roles that the user is a direct member of.
                        var memberPage = await graph.Me.MemberOf.Request().Select(select).GetAsync().ConfigureAwait(false);

                        if (memberPage?.Count > 0)
                        {
                            //There is a limit to number of groups returned, below method make calls to Microsoft graph to get all the groups.
                            var allgroups = ProcessIGraphServiceMemberOfCollectionPage(memberPage);

                            if (allgroups?.Count > 0)
                            {
                                var identity = (ClaimsIdentity)context.Principal.Identity;

                                if (identity != null)
                                {
                                    //Remove existing groups claims
                                    RemoveExistingClaims(context, identity);

                                    List<Claim> groupClaims = new List<Claim>();

                                    foreach (Group group in allgroups)
                                    {
                                        //Claim is added in list and it can be used by saving the groups in session or as per project implementation.
                                        //Adds group id as 'groups' claim. But it can be changed as per requirment. 
                                        //For instance if the required format is 'NetBIOSDomain\sAMAccountName' then the code is as commented below:
                                        //groupClaims.AddClaim(new Claim("groups", group.OnPremisesNetBiosName+"\\"+group.OnPremisesSamAccountName));
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
                    //Remove the key as Microsoft.Identity.Web library utilizes this key. 
                    //If not removed then it can cause failure to the application.
                    context.HttpContext.Items.Remove("JwtSecurityTokenUsedToCallWebAPI");
                }
            }
        }

        /// <summary>
        /// Remove groups claims if already exists.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="identity"></param>
        private static void RemoveExistingClaims(TokenValidatedContext context, ClaimsIdentity identity)
        {
            //clear existing claim
            List<Claim> existingGroupsClaims = context.Principal.Claims.Where(x => x.Type == "groups").ToList();
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
