using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace CallMSGraph.Helpers
{
    /// <summary>
    /// Helper methods to work with data in WWW-Authentication header
    /// </summary>
    public class AuthenticationHeaderHelper
    {
        /// <summary>
        /// Extracts the claim challenge from HTTP header.
        /// </summary>
        /// <param name="httpResponseHeaders">The HTTP response headers.</param>
        /// <returns></returns>
        internal static string ExtractClaimChallengeFromHttpHeader(HttpResponseHeaders httpResponseHeaders)
        {
            if (httpResponseHeaders.WwwAuthenticate.Any())
            {
                AuthenticationHeaderValue bearer = httpResponseHeaders.WwwAuthenticate.First(v => v.Scheme == "Bearer");
                IEnumerable<string> parameters = bearer.Parameter.Split(',').Select(v => v.Trim()).ToList();
                var errorValue = GetParameterValue(parameters, "error");

                try
                {
                    // read the header and checks if it conatins error with insufficient_claims value.
                    if (null != errorValue && "insufficient_claims" == errorValue)
                    {
                        var claimChallengeParameter = GetParameterValue(parameters, "claims");
                        if (null != claimChallengeParameter)
                        {
                            var claimChallenge = ConvertBase64String(claimChallengeParameter);

                            return claimChallenge;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return null;
        }

        private static string GetParameterValue(IEnumerable<string> parameters, string parameterName)
        {
            int offset = parameterName.Length + 1;
            return parameters.FirstOrDefault(p => p.StartsWith($"{parameterName}="))?.Substring(offset)?.Trim('"');
        }

        /// <summary>
        /// Checks and if input is base-64 encoded string then decodes it.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private static string ConvertBase64String(string inputString)
        {
            if (inputString == null || inputString.Length == 0 || inputString.Length % 4 != 0 || inputString.Contains(" ") || inputString.Contains("\t") || inputString.Contains("\r") || inputString.Contains("\n"))
            {
                return inputString;
            }

            try
            {
                var claimChallengebase64Bytes = Convert.FromBase64String(inputString);
                var claimChallenge = System.Text.Encoding.UTF8.GetString(claimChallengebase64Bytes);
                return claimChallenge;
            }
            catch (Exception)
            {
                return inputString;
            }
        }
    }
}
