﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Identity.Web
{
    public static class AuthorityHelpers
    {
        internal static bool IsV2Authority(string authority)
        {
            if (string.IsNullOrEmpty(authority))
                return false;

            return authority.EndsWith("/v2.0");
        }

        internal static string BuildAuthority(MicrosoftIdentityOptions options)
        {
            if (options == null)
                return null;

            // Cannot build authority without AAD Intance or Domain
            if (string.IsNullOrWhiteSpace(options.Instance) || string.IsNullOrWhiteSpace(options.Domain))
                return null;

            var baseUri = new Uri(options.Instance);
            var pathBase = baseUri.PathAndQuery.TrimEnd('/');
            var domain = options.Domain;

            // If there are policies, then it must build a B2C authority 
            if (!string.IsNullOrWhiteSpace(options.DefaultPolicy))
            {
                var policy = options.DefaultPolicy;
                return new Uri(baseUri, new PathString($"{pathBase}/{domain}/{policy}/v2.0")).ToString();
            }

            else
                return new Uri(baseUri, new PathString($"{pathBase}/{domain}/v2.0")).ToString();
        }
    }
}