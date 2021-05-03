using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace _2_1_Call_MSGraph.Models
{
    public class AuthenticationHeaderHelper
    {
        /// <summary>
        /// Extract claims from WwwAuthenticate header and returns the value.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static string ExtractHeaderValues(WebApiMsalUiRequiredException response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && response.Headers.WwwAuthenticate.Any())
            {
                AuthenticationHeaderValue bearer = response.Headers.WwwAuthenticate.First(v => v.Scheme == "Bearer");
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

        internal static string ExtractHeaderValues(HttpResponseHeaders httpResponseHeaders)
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
