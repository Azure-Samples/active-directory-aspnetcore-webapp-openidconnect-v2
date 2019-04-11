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

using Microsoft.Graph;
using System.Collections.Generic;

namespace WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph
{
    /// <summary>
    /// An entity class that holds both groups and roles for a user.
    /// </summary>
    public class UserGroupsAndDirectoryRoles
    {
        public UserGroupsAndDirectoryRoles()
        {
            this.GroupIds = new List<string>();
            this.Groups = new List<Group>();
            this.DirectoryRoles = new List<DirectoryRole>();
        }

        /// <summary>Gets or sets a value indicating whether this user's groups claim will result in an overage </summary>
        /// <value>
        ///   <c>true</c> if this instance has overage claim; otherwise, <c>false</c>.</value>
        public bool HasOverageClaim { get; set; }

        /// <summary>Gets or sets the group ids.</summary>
        /// <value>The group ids.</value>
        public List<string> GroupIds { get; set; }

        /// <summary>Gets or sets the groups.</summary>
        /// <value>The groups.</value>
        public List<Group> Groups { get; set; }

        /// <summary>Gets or sets the App roles</summary>
        public List<DirectoryRole> DirectoryRoles { get; set; }
    }
}