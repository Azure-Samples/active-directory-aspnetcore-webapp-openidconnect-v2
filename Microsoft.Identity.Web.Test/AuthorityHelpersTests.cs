using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Identity.Web.Test
{
    public class AuthorityHelpersTests
    {
        [Fact]
        public void IsV2Authority_EmptyParam_ReturnsFalse()
        {
            //Arrange
            string authority = string.Empty;
            bool? result = null;

            //Act
            result = AuthorityHelpers.IsV2Authority(authority);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void IsV2Authority_NullParam_ReturnsFalse()
        {
            //Arrange
            string authority = null;
            bool? result = null;

            //Act
            result = AuthorityHelpers.IsV2Authority(authority);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void IsV2Authority_EndsWithV2_ReturnsTrue()
        {
            //Arrange
            string authority = "https://login.microsoftonline.com/tenantId/v2.0";
            bool? result = null;

            //Act
            result = AuthorityHelpers.IsV2Authority(authority);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void IsV2Authority_DoesntEndWithV2_ReturnsFalse()
        {
            //Arrange
            string authority = "https://login.microsoftonline.com/tenantId";
            bool? result = null;

            //Act
            result = AuthorityHelpers.IsV2Authority(authority);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void BuildAuthority_NullOptions_ReturnsNull()
        {
            //Arrange
            MicrosoftIdentityOptions options = null;
            string result = null;

            //Act
            result = AuthorityHelpers.BuildAuthority(options);

            //Assert
            Assert.Null(result);

        }

        [Fact]
        public void BuildAuthority_EmptyInstance_ReturnsNull()
        {
            //Arrange
            MicrosoftIdentityOptions options = new MicrosoftIdentityOptions();
            options.Domain = "contoso.onmicrosoft.com";
            options.Instance = "";
            string result = null;

            //Act
            result = AuthorityHelpers.BuildAuthority(options);

            //Assert
            Assert.Null(result);

        }

        [Fact]
        public void BuildAuthority_EmptyDomain_ReturnsNull()
        {
            //Arrange
            MicrosoftIdentityOptions options = new MicrosoftIdentityOptions();
            options.Domain = "";
            options.Instance = "https://login.microsoftonline.com/";
            string result = null;

            //Act
            result = AuthorityHelpers.BuildAuthority(options);

            //Assert
            Assert.Null(result);

        }

        [Fact]
        public void BuildAuthority_AadInstanceAndDomain_BuildAadAuthority()
        {
            //Arrange
            MicrosoftIdentityOptions options = new MicrosoftIdentityOptions();
            options.Domain = "contoso.onmicrosoft.com";
            options.Instance = "https://login.microsoftonline.com";
            string result = null;
            string expectedResult = $"{options.Instance}/{options.Domain}/v2.0";

            //Act
            result = AuthorityHelpers.BuildAuthority(options);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(result, expectedResult);
        }

        [Fact]
        public void BuildAuthority_OptionsInstaceWithTrailing_BuildAadAuthorityWithoutExtraTrailing()
        {
            //Arrange
            MicrosoftIdentityOptions options = new MicrosoftIdentityOptions();
            options.Domain = "contoso.onmicrosoft.com";
            options.Instance = "https://login.microsoftonline.com/";
            string result = null;
            string expectedResult = $"https://login.microsoftonline.com/{options.Domain}/v2.0";

            //Act
            result = AuthorityHelpers.BuildAuthority(options);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(result, expectedResult);
        }

        [Fact]
        public void BuildAuthority_B2CInstanceDomainAndPolicy_BuildB2CAuthority()
        {
            //Arrange
            MicrosoftIdentityOptions options = new MicrosoftIdentityOptions();
            options.Domain = "fabrikamb2c.onmicrosoft.com";
            options.Instance = "https://fabrikamb2c.b2clogin.com";
            options.SignUpSignInPolicyId = "b2c_1_susi";

            string result = null;
            string expectedResult = $"{options.Instance}/{options.Domain}/{options.DefaultPolicy}/v2.0";

            //Act
            result = AuthorityHelpers.BuildAuthority(options);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(result, expectedResult);
        }
    }
}
