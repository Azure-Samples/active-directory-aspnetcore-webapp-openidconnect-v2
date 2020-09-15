using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    public class GroupPolicyHandler : AuthorizationHandler<GroupPolicyRequirement>
    {
        private IHttpContextAccessor _httpContextAccessor;

        public GroupPolicyHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   GroupPolicyRequirement requirement)
        {
            // Checks if groups claim exists in claims collection of signed-in User.
            if (context.User.Claims.Any(x => x.Type == "groups"))
            {
                if (context.User.Claims.Any(x => x.Type == "groups" && x.Value == requirement.GroupName))
                {
                    context.Succeed(requirement);
                }
                return Task.CompletedTask;
            }

            // Checks if Session contains data for groupClaims.
            // The data will exist for 'Group Overage' claim.
            else if (_httpContextAccessor.HttpContext.Session.Keys.Contains("groupClaims"))
            {
                // Retrieves all the groups saved in Session.
                var groups = _httpContextAccessor.HttpContext.Session.GetAsByteArray("groupClaims") as List<string>;

                // Checks if required group exists in Session.
                if (groups?.Count > 0 && groups.Contains(requirement.GroupName))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
    public class GroupPolicyRequirement : IAuthorizationRequirement
    {
        public string GroupName { get; }
        public GroupPolicyRequirement(string GroupName)
        {
            this.GroupName = GroupName;
        }
    }
}
