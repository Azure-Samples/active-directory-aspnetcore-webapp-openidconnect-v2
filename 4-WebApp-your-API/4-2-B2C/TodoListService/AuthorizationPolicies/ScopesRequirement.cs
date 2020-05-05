// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
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
            // If there are no scopes, do not process
            if (!context.User.Claims.Any(x => x.Type == ClaimConstants.Scope)
               && !context.User.Claims.Any(y => y.Type == ClaimConstants.Scp))
            {
                return Task.CompletedTask;
            }

            Claim scopeClaim = context?.User?.FindFirst(ClaimConstants.Scp);

            if (scopeClaim == null)
                scopeClaim = context?.User?.FindFirst(ClaimConstants.Scope);

            if (scopeClaim != null && scopeClaim.Value.Split(' ').Intersect(requirement._acceptedScopes).Any())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
