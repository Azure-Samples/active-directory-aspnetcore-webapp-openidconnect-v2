using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extension methods that don't fit in any other class
    /// </summary>
    public static class Extensions
    {
        /// <summary>Determines whether the specified string collection contains any.</summary>
        /// <param name="searchFor">The search for.</param>
        /// <param name="stringCollection">The string collection.</param>
        /// <returns>
        ///   <c>true</c> if the specified string collection contains any; otherwise, <c>false</c>.</returns>
        public static bool ContainsAny(this string searchFor, params string[] stringCollection)
        {
            foreach (string str in stringCollection)
            {
                if (searchFor.Contains(str))
                    return true;
            }

            return false;
        }
    }
}
