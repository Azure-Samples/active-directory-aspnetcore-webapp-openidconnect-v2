/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph
{
    /// <summary>
    /// IMSGraph service has samples to show how to call Microsoft Graph using the Graph SDK
    /// </summary>
    public interface IMSGraphService
    {
        /// <summary>Gets the current user directory roles.</summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>A list of directory roles</returns>
        Task<IList<DirectoryRole>> GetCurrentUserDirectoryRolesAsync(string accessToken);

        /// <summary>Gets the signed-in user groups and roles. A more efficient implementation that gets both group and role membership in one call</summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>A list of UserGroupsAndDirectoryRoles</returns>
        Task<UserGroupsAndDirectoryRoles> GetCurrentUserGroupsAndRolesAsync(string accessToken);

        /// <summary>Gets the groups the signed-in user's is a member of.</summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>A list of Groups</returns>
        Task<IList<Group>> GetCurrentUsersGroupsAsync(string accessToken);

        /// <summary>Gets basic details about the signed-in user.</summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>A detail of the User object</returns>
        Task<User> GetMeAsync(string accessToken);

        /// <summary>Gets the groups the signed-in user's is a direct member of.</summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>A list of Groups</returns>
        Task<List<Group>> GetMyMemberOfGroupsAsync(string accessToken);

        /// <summary>Gets the signed-in user's photo.</summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>The photo of the signed-in user as a base64 string</returns>
        Task<string> GetMyPhotoAsync(string accessToken);

        /// <summary>Gets the users in a tenant.</summary>
        /// <param name="accessToken">The access token for MS Graph.</param>
        /// <returns>A list of users</returns>
        Task<List<User>> GetUsersAsync(string accessToken);
    }
}