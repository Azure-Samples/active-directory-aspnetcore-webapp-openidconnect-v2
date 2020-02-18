﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace Microsoft.Identity.Web.Test
{
    public class WebApiServiceCollectionExtensionsTests
    {
        [Fact]
        public void TestAuthority()
        {
            // Arrange
            JwtBearerOptions options = new JwtBearerOptions();

            // Act and Assert
            options.Authority = "https://login.microsoftonline.com/common";
            WebApiServiceCollectionExtensions.EnsureAuthorityIsV2_0(options);
            Assert.Equal("https://login.microsoftonline.com/common/v2.0", options.Authority);

            options.Authority = "https://login.microsoftonline.us/organizations";
            WebApiServiceCollectionExtensions.EnsureAuthorityIsV2_0(options);
            Assert.Equal("https://login.microsoftonline.us/organizations/v2.0", options.Authority);

            options.Authority = "https://login.microsoftonline.com/common/";
            WebApiServiceCollectionExtensions.EnsureAuthorityIsV2_0(options);
            Assert.Equal("https://login.microsoftonline.com/common/v2.0", options.Authority);

            options.Authority = "https://login.microsoftonline.com/common/v2.0";
            WebApiServiceCollectionExtensions.EnsureAuthorityIsV2_0(options);
            Assert.Equal("https://login.microsoftonline.com/common/v2.0", options.Authority);


            options.Authority = "https://login.microsoftonline.com/common/v2.0";
            WebApiServiceCollectionExtensions.EnsureAuthorityIsV2_0(options);
            Assert.Equal("https://login.microsoftonline.com/common/v2.0", options.Authority);

        }

        [Fact]
        public void TestAudience()
        {
            JwtBearerOptions options = new JwtBearerOptions();
            MicrosoftIdentityOptions microsoftIdentityOptions = new MicrosoftIdentityOptions() { ClientId = Guid.NewGuid().ToString() };

            // Act and Assert
            options.Audience = "https://localhost";
            WebApiServiceCollectionExtensions.EnsureValidAudiencesContainsApiGuidIfGuidProvided(options, microsoftIdentityOptions);
            Assert.True(options.TokenValidationParameters.ValidAudiences.Count() == 1);
            Assert.True(options.TokenValidationParameters.ValidAudiences.First() == "https://localhost");

            options.Audience = "api://1EE5A092-0DFD-42B6-88E5-C517C0141321";
            WebApiServiceCollectionExtensions.EnsureValidAudiencesContainsApiGuidIfGuidProvided(options, microsoftIdentityOptions);
            Assert.True(options.TokenValidationParameters.ValidAudiences.Count() == 1);
            Assert.True(options.TokenValidationParameters.ValidAudiences.First() == "api://1EE5A092-0DFD-42B6-88E5-C517C0141321");

            options.Audience = "1EE5A092-0DFD-42B6-88E5-C517C0141321";
            WebApiServiceCollectionExtensions.EnsureValidAudiencesContainsApiGuidIfGuidProvided(options, microsoftIdentityOptions);
            Assert.True(options.TokenValidationParameters.ValidAudiences.Count() == 2);
            Assert.Contains("api://1EE5A092-0DFD-42B6-88E5-C517C0141321", options.TokenValidationParameters.ValidAudiences);
            Assert.Contains("1EE5A092-0DFD-42B6-88E5-C517C0141321", options.TokenValidationParameters.ValidAudiences);

            options.Audience = null;
            WebApiServiceCollectionExtensions.EnsureValidAudiencesContainsApiGuidIfGuidProvided(options, microsoftIdentityOptions);
            Assert.True(options.TokenValidationParameters.ValidAudiences.Count() == 2);
            Assert.Contains($"api://{microsoftIdentityOptions.ClientId}", options.TokenValidationParameters.ValidAudiences);
            Assert.Contains($"{microsoftIdentityOptions.ClientId}", options.TokenValidationParameters.ValidAudiences);

        }
    }
}
