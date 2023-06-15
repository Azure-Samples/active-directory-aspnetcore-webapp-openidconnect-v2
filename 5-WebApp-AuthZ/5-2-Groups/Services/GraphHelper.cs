using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Me.GetMemberGroups;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;

namespace WebApp_OpenIDConnect_DotNet.Services
{
    /// <summary>
    /// Contains a set of methods and a pattern on how to handle group claims overage
    /// </summary>
    public class GraphHelper
    {
        private const string Cached_Graph_Token_Key = "JwtSecurityTokenUsedToCallWebAPI";
        private const string Groups_Cache_Key = "groupClaims_";
        private static IMemoryCache _memoryCache;

        /// <summary>
        /// This method inspects the claims collection created from the ID or Access token issued to a user and returns the groups that are present in the token.
        /// If it detects groups overage, the method then makes calls to ProcessUserGroupsForOverage method to get the entire set of security groups and populates
        /// the Claim Principal's "groups" claim with the complete list of groups.
        /// </summary>
        /// <param name="context">TokenValidatedContext</param>
        /// <param name="requiredGroups">List</param>
        public static async Task ProcessAnyGroupsOverage(TokenValidatedContext context, List<string> requiredGroupIds, CacheSettings cacheSettings)
        {
            ClaimsPrincipal principal = context.Principal;

            if (principal == null || principal.Identity == null)
            {
                await Task.CompletedTask;
            }

            // ensure MemoryCache is available
            _memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();

            if (_memoryCache == null)
            {
                throw new ArgumentNullException("_memoryCache", "Memory cache is not available.");
            }

            // Checks if the incoming token contains a 'Group Overage' claim.
            if (HasOverageOccurred(principal))
            {
                // Gets group values from cache if available.
                var usergroups = GetUserGroupsFromCache(principal);

                if (usergroups == null || usergroups.Count == 0) // Cache eviction
                {
                    usergroups = await ProcessUserGroupsForOverage(context, requiredGroupIds);
                }

                // Populate the current ClaimsPrincipal 'groups' claim with all the groups to ensure that policy check works as expected
                if (usergroups?.Count > 0)
                {
                    var identity = (ClaimsIdentity)principal.Identity;

                    // Remove any existing 'groups' claim
                    RemoveExistingGroupsClaims(identity);

                    // And re-populate
                    RepopulateGroupsClaim(usergroups, identity);

                    // Here we add the groups in a cache variable so that calls to Graph can be minimized to fetch all the groups for a user.
                    // IMPORTANT: Group list is cached for 1 hr by default, and thus cached groups will miss any changes to a users group membership for this duration.
                    // For capturing real-time changes to a user's group membership, consider implementing MS Graph change notifications (https://learn.microsoft.com/graph/api/resources/webhooks)
                    SaveUsersGroupsToCache(usergroups, principal, cacheSettings);
                }
            }
        }

        /// <summary>
        /// Checks if 'Group Overage' claim exists for signed-in user.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private static bool HasOverageOccurred(ClaimsPrincipal identity)
        {
            return identity.Claims.Any(x => x.Type == "hasgroups" || (x.Type == "_claim_names" && x.Value == "{\"groups\":\"src1\"}"));
        }

        /// <summary>
        /// ID Token does not contain 'scp' claim.
        /// This claims only exists in the Access Token.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private static bool IsAccessToken(ClaimsIdentity identity)
        {
            return identity.Claims.Any(x => x.Type == "scp" || x.Type == "http://schemas.microsoft.com/identity/claims/scope");
        }

        /// <summary>
        /// This method inspects the claims collection created from the ID or Access token issued to a user and returns the groups that are present in the token . If it detects groups overage,
        /// the method then makes calls to Microsoft Graph to fetch the group membership of the authenticated user.
        /// </summary>
        /// <param name="context">TokenValidatedContext</param>
        private static async Task<List<string>> ProcessUserGroupsForOverage(TokenValidatedContext context, List<string> requiredGroupIds)
        {
            var allgroups = new List<string>();

            try
            {
                // Before instantiating GraphServiceClient, the app should have granted admin consent for 'GroupMember.Read.All' permission.
                var graphClient = context.HttpContext.RequestServices.GetService<GraphServiceClient>();

                if (graphClient == null)
                {
                    throw new ArgumentNullException("GraphServiceClient", "No service for type 'Microsoft.Graph.GraphServiceClient' has been registered in the Startup.");
                }

                // Checks if the SecurityToken is not null.
                // For the Web Api, SecurityToken contains claims from the Access Token.
                if (context.SecurityToken != null)
                {
                    // Checks if 'JwtSecurityTokenUsedToCallWebAPI' key already exists.
                    // This key is required to acquire Access Token for Graph Service Client.
                    if (!context.HttpContext.Items.ContainsKey(Cached_Graph_Token_Key))
                    {
                        // For Web App, access token is retrieved using account identifier. But at this point account identifier is null.
                        // So, SecurityToken is saved in 'JwtSecurityTokenUsedToCallWebAPI' key.
                        // The key is then used to get the Access Token on-behalf of user.
                        context.HttpContext.Items.Add(Cached_Graph_Token_Key, context.SecurityToken as JwtSecurityToken);
                    }

                    try
                    {
                        // Request to get groups and directory roles that the user is a direct member of.
                        var memberPage = await graphClient.Me.GetMemberGroups.PostAsync(new GetMemberGroupsPostRequestBody() { SecurityEnabledOnly = false});
                        allgroups = memberPage.Value.ToList<string>();

                        if (allgroups?.Count > 0)
                        {
                            var principal = context.Principal;

                            if (principal != null)
                            {
                                var identity = principal.Identity as ClaimsIdentity;

                                // Remove existing groups claims
                                RemoveExistingGroupsClaims(identity);

                                // And re-populate
                                RepopulateGroupsClaim(allgroups, identity);
                            }

                            // return the full list of security groups
                            return allgroups;
                        }
                    }
                    catch (Exception graphEx)
                    {
                        var exMsg = graphEx.InnerException != null ? graphEx.InnerException.Message : graphEx.Message;
                        Console.WriteLine("Call to Microsoft Graph failed: " + exMsg);
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
                if (context.HttpContext.Items.ContainsKey(Cached_Graph_Token_Key))
                {
                    // Removes 'JwtSecurityTokenUsedToCallWebAPI' from Items collection.
                    // If not removed then it can cause failure to the application.
                    // Because this key is also added by StoreTokenUsedToCallWebAPI method of Microsoft.Identity.Web.
                    context.HttpContext.Items.Remove(Cached_Graph_Token_Key);
                }
            }

            return null;
        }

        /// <summary>
        /// Re-populate the `groups` claim with the complete list of groups fetched from MS Graph
        /// </summary>
        /// <param name="allgroups">The user's entire security group membership.</param>
        /// <param name="identity">The identity.</param>
        /// <autogeneratedoc />
        private static void RepopulateGroupsClaim(List<string> allgroups, ClaimsIdentity identity)
        {
            foreach (string group in allgroups)
            {
                // The following code adds group ids to the 'groups' claim. But depending upon your requirement and the format of the 'groups' claim selected in
                // the app registration, you might want to add other attributes than id to the `groups` claim, examples being;

                // For instance if the required format is 'NetBIOSDomain\sAMAccountName' then the code is as commented below:
                // identity.AddClaim(new Claim("groups", group.OnPremisesNetBiosName+"\\"+group.OnPremisesSamAccountName));
                identity.AddClaim(new Claim("groups", group));
            }
        }

        /// <summary>
        /// Remove groups claims if already exists.
        /// </summary>
        /// <param name="identity"></param>
        private static void RemoveExistingGroupsClaims(ClaimsIdentity identity)
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
        /// Gets the signed-in user's object identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        /// <autogeneratedoc />
        private static string GetUserObjectId(ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == "oid").Value;
        }

        /// <summary>
        /// Retrieves all the groups saved in Cache.
        /// </summary>>
        /// <returns></returns>
        private static List<string> GetUserGroupsFromCache(ClaimsPrincipal principal)
        {
            // Checks if Session contains data for groupClaims.
            // The data will exist for 'Group Overage' claim if already populated.
            string cacheKey = $"{Groups_Cache_Key}{GetUserObjectId(principal)}";

            if (_memoryCache.TryGetValue(cacheKey, out List<string> groups))
            {
                Debug.WriteLine($"Cache hit successful for '{cacheKey}'");
                return groups;
            }

            return null;
        }

        /// <summary>
        /// Saves the users groups to the memory cache.
        /// </summary>
        /// <param name="usersGroups">The users groups to cache.</param>
        /// <param name="principal">The Claims principal.</param>
        /// <autogeneratedoc />
        private static void SaveUsersGroupsToCache(List<string> usersGroups, ClaimsPrincipal principal, CacheSettings cacheSettings)
        {
            string cacheKey = $"{Groups_Cache_Key}{GetUserObjectId(principal)}";

            Console.WriteLine($"Adding users groups for '{cacheKey}'.");

            // IMPORTANT: Group list is cached for 1 hr by default, and thus cached groups will miss any changes to a users group membership for this duration.
            // For capturing real-time changes to a user's group membership, consider implementing MS Graph change notifications (https://learn.microsoft.com/en-us/graph/api/resources/webhooks)
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(Convert.ToDouble(cacheSettings.SlidingExpirationInSeconds)))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(Convert.ToDouble(cacheSettings.AbsoluteExpirationInSeconds)))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(10240);

            _memoryCache.Set(cacheKey, usersGroups, cacheEntryOptions);
        }
    }
}