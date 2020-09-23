using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Services;

namespace WebApp_OpenIDConnect_DotNet.Infrastructure
{
    /// <summary>
    /// GroupPolicyHandler deals with custom Policy-based authorization.
    /// GroupPolicyHandler evaluates the GroupPolicyRequirement against AuthorizationHandlerContext 
    /// by calling CheckUsersGroupMembership method to determine if authorization is allowed.
    /// </summary>
    public class GroupPolicyHandler : AuthorizationHandler<GroupPolicyRequirement>
    {
        private IHttpContextAccessor _httpContextAccessor;

        public GroupPolicyHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Makes a decision if authorization is allowed based on GroupPolicyRequirement.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   GroupPolicyRequirement requirement)
        {
            // Calls method to check if requirement exists in user claims or session.
            if (GraphHelper.CheckUsersGroupMembership(context, requirement.GroupName, _httpContextAccessor))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// GroupPolicyRequirement contains data parameter that 
    /// GroupPolicyHandler uses to evaluate against the current user principal or session data.
    /// </summary>
    public class GroupPolicyRequirement : IAuthorizationRequirement
    {
        public string GroupName { get; }
        public GroupPolicyRequirement(string GroupName)
        {
            this.GroupName = GroupName;
        }
    }
}
