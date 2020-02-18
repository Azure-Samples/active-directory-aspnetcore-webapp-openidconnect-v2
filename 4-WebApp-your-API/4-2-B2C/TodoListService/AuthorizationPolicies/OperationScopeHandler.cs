using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Identity.Web;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TodoListService.AuthorizationPolicies
{
    /// <summary>
    /// AuthorizationHandler that will check if the scope claim has the requirement value
    /// </summary>
    public class OperationScopeHandler : AuthorizationHandler<OperationAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       OperationAuthorizationRequirement requirement)
        {
            if (!context.User.Claims.Any(x => x.Type == ClaimConstants.Scope)
                   && !context.User.Claims.Any(y => y.Type == ClaimConstants.Scp))
            {
                return Task.CompletedTask;
            }

            Claim scopeClaim = context?.User?.FindFirst(ClaimConstants.Scp);

            if(scopeClaim == null)
                scopeClaim = context?.User?.FindFirst(ClaimConstants.Scope);

            if (scopeClaim != null && scopeClaim.Value.Split(' ').Contains(requirement.Name))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
