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
    }
}
