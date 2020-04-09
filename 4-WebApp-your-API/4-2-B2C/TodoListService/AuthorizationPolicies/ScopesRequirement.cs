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
    /// Requirement used in authorization policies, to check if the scope claim has at least one of the requirement values.
    /// Since the class also extends AuthorizationHandler, its dependency injection is done out of the box.
    /// </summary>
    public class ScopesRequirement : AuthorizationHandler<ScopesRequirement>, IAuthorizationRequirement
    {
        string[] _acceptedScopes;

        public ScopesRequirement(params string[] acceptedScopes)
        {
            _acceptedScopes = acceptedScopes;
        }

        /// <summary>
        /// AuthorizationHandler that will check if the scope claim has at least one of the requirement values
        /// </summary>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                        ScopesRequirement requirement)
        {
             string scope = "http://schemas.microsoft.com/identity/claims/scope";
             string scp = "scp";
            // If there are no scopes, do not process
            if (!context.User.Claims.Any(x => x.Type == scope)
               && !context.User.Claims.Any(y => y.Type == scp))
            {
                return Task.CompletedTask;
            }

            Claim scopeClaim = context?.User?.FindFirst(scp);

            if (scopeClaim == null)
                scopeClaim = context?.User?.FindFirst(scope);

            if (scopeClaim != null && scopeClaim.Value.Split(' ').Intersect(requirement._acceptedScopes).Any())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
