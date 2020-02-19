// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Microsoft.Identity.Web.Test
{
    public class AadIssuerValidatorTests
    {
        private const string Tid = "9188040d-6c67-4c5b-b112-36a304b66dad";
        private static readonly string Iss = $"https://login.microsoftonline.com/{Tid}/v2.0";
        private static readonly IEnumerable<string> s_aliases = new[] { "login.microsoftonline.com", "sts.windows.net" };

        [Fact]
        public void NullArg()
        {
            // Arrange
            AadIssuerValidator validator = new AadIssuerValidator(s_aliases);
            var jwtSecurityToken = new JwtSecurityToken();
            var validationParams = new TokenValidationParameters();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => validator.Validate(null, jwtSecurityToken, validationParams));
            Assert.Throws<ArgumentNullException>(() => validator.Validate("", jwtSecurityToken, validationParams));
            Assert.Throws<ArgumentNullException>(() => validator.Validate(Iss, null, validationParams));
            Assert.Throws<ArgumentNullException>(() => validator.Validate(Iss, jwtSecurityToken, null));
        }

        [Fact]
        public void PassingValidation()
        {
            // Arrange
            AadIssuerValidator validator = new AadIssuerValidator(s_aliases);
            Claim issClaim = new Claim("tid", Tid);
            Claim tidClaim = new Claim("iss", Iss);
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: Iss, claims: new[] { issClaim, tidClaim });

            // Act & Assert
            validator.Validate(Iss, jwtSecurityToken,
                new TokenValidationParameters() { ValidIssuers = new[] { "https://login.microsoftonline.com/{tenantid}/v2.0" } });
        }


        [Fact]
        public void TokenValidationParameters_ValidIssuer()
        {
            // Arrange
            AadIssuerValidator validator = new AadIssuerValidator(s_aliases);
            Claim issClaim = new Claim("tid", Tid);
            Claim tidClaim = new Claim("iss", Iss);
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: Iss, claims: new[] { issClaim, tidClaim });

            // Act & Assert
            validator.Validate(Iss, jwtSecurityToken,
                new TokenValidationParameters() { ValidIssuer = "https://login.microsoftonline.com/{tenantid}/v2.0" });
        }

        [Fact]
        public void ValidationSucceeds_NoTidClaimInJwt_TidCreatedFromIssuerInstead()
        {
            // Arrange
            AadIssuerValidator validator = new AadIssuerValidator(s_aliases);
            Claim issClaim = new Claim("iss", Iss);

            JwtSecurityToken noTidJwt = new JwtSecurityToken(issuer: Iss, claims: new[] { issClaim });

            // Act & Assert
                validator.Validate(
                    Iss,
                    noTidJwt,
                    new TokenValidationParameters() { ValidIssuers = new[] { "https://login.microsoftonline.com/{tenantid}/v2.0" } });
        }

        [Fact]
        public void ValidationFails_BadTidClaimInJwt()
        {
            // Arrange
            AadIssuerValidator validator = new AadIssuerValidator(s_aliases);
            Claim issClaim = new Claim("iss", Iss);
            Claim tidClaim = new Claim("tid", "9188040d-0000-4c5b-b112-36a304b66dad");

            JwtSecurityToken noTidJwt = new JwtSecurityToken(issuer: Iss, claims: new[] { issClaim, tidClaim });

            // Act & Assert
            Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
                validator.Validate(
                    Iss,
                    noTidJwt,
                    new TokenValidationParameters() { ValidIssuers = new[] { "https://login.microsoftonline.com/{tenantid}/v2.0" } }));
        }

        [Fact]
        public void MultipleIssuers_NoneMatch()
        {
            // Arrange
            AadIssuerValidator validator = new AadIssuerValidator(s_aliases);
            Claim issClaim = new Claim("iss", Iss);
            Claim tidClaim = new Claim("tid", Tid);

            JwtSecurityToken noTidJwt = new JwtSecurityToken(issuer: Iss, claims: new[] { issClaim, tidClaim });

            // Act & Assert
            Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
                validator.Validate(
                    Iss,
                    noTidJwt,
                    new TokenValidationParameters()
                    {
                        ValidIssuers = new[] {
                        "https://host1/{tenantid}/v2.0",
                        "https://host2/{tenantid}/v2.0"
                        }
                    }));
        }


        [Fact] // Regression test for https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/issues/68
        public void ValidationFails_BadIssuerClaimInJwt()
        {
            // Arrange
            string iss = $"https://badissuer/{Tid}/v2.0";
            AadIssuerValidator validator = new AadIssuerValidator(s_aliases);
            Claim issClaim = new Claim("iss", iss);
            Claim tidClaim = new Claim("tid", Tid);

            JwtSecurityToken noTidJwt = new JwtSecurityToken(issuer: Iss, claims: new[] { issClaim, tidClaim });

            // Act & Assert
            Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
                validator.Validate(
                    iss,
                    noTidJwt,
                    new TokenValidationParameters() { ValidIssuers = new[] { "https://login.microsoftonline.com/{tenantid}/v2.0" } }));
        }

        [Fact]
        public void Validate_FromB2CAuthority_WithNoTidClaim_ValidateSuccessfully()
        {
            //Arrange
            string b2cAuthority = "https://fabrikamb2c.b2clogin.com/fabrikamb2c.onmicrosoft.com/b2c_1_susi/v2.0";
            string issuer = "https://fabrikamb2c.b2clogin.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/";
            Claim issClaim = new Claim("iss", issuer);
            Claim tfpClaim = new Claim("tfp", "b2c_1_susi");
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: issuer, claims: new[] { issClaim, tfpClaim });

            //Act
            AadIssuerValidator validator = AadIssuerValidator.GetIssuerValidator(b2cAuthority);

            //Assert
            validator.Validate(
                issuer,
                jwtSecurityToken,
                new TokenValidationParameters()
                {
                    ValidIssuers = new[] { "https://fabrikamb2c.b2clogin.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/" }
                });
        }

        [Fact]
        public void Validate_FromB2CAuthority_WithTidClaim_ValidateSuccessfully()
        {
            //Arrange
            string b2cAuthority = "https://fabrikamb2c.b2clogin.com/fabrikamb2c.onmicrosoft.com/b2c_1_susi/v2.0";
            string issuer = "https://fabrikamb2c.b2clogin.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/";
            Claim issClaim = new Claim("iss", issuer);
            Claim tidClaim = new Claim("tid", "775527ff-9a37-4307-8b3d-cc311f58d925");
            Claim tfpClaim = new Claim("tfp", "b2c_1_susi");
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: issuer, claims: new[] { issClaim, tfpClaim, tidClaim });

            //Act
            AadIssuerValidator validator = AadIssuerValidator.GetIssuerValidator(b2cAuthority);

            //Assert
            validator.Validate(
                issuer,
                jwtSecurityToken,
                new TokenValidationParameters()
                {
                    ValidIssuers = new[] { "https://fabrikamb2c.b2clogin.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/" }
                });
        }

        [Fact]
        public void Validate_FromB2CAuthority_InvalidIssuer_Fails ()
        {
            //Arrange
            string b2cAuthority = "https://fabrikamb2c.b2clogin.com/fabrikamb2c.onmicrosoft.com/b2c_1_susi/v2.0";
            string badIssuer = "https://badIssuer.b2clogin.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/";
            Claim issClaim = new Claim("iss", badIssuer);
            Claim tfpClaim = new Claim("tfp", "b2c_1_susi");
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: badIssuer, claims: new[] { issClaim, tfpClaim });

            //Act
            AadIssuerValidator validator = AadIssuerValidator.GetIssuerValidator(b2cAuthority);

            //Assert
            Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
                validator.Validate(
                    badIssuer,
                    jwtSecurityToken,
                    new TokenValidationParameters()
                    {
                        ValidIssuers = new[] { "https://fabrikamb2c.b2clogin.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/" }
                    })
                );
        }

        [Fact]
        public void Validate_FromB2CAuthority_InvalidIssuerTid_Fails()
        {
            //Arrange
            string wrongTid = "9188040d-6c67-4c5b-b112-36a304b66dad";
            string b2cAuthority = "https://fabrikamb2c.b2clogin.com/fabrikamb2c.onmicrosoft.com/b2c_1_susi/v2.0";
            string badIssuer = $"https://fabrikamb2c.b2clogin.com/{wrongTid}/v2.0/";
            Claim issClaim = new Claim("iss", badIssuer);
            Claim tfpClaim = new Claim("tfp", "b2c_1_susi");
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: badIssuer, claims: new[] { issClaim, tfpClaim });

            //Act
            AadIssuerValidator validator = AadIssuerValidator.GetIssuerValidator(b2cAuthority);

            //Assert
            Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
                validator.Validate(
                    badIssuer,
                    jwtSecurityToken,
                    new TokenValidationParameters()
                    {
                        ValidIssuers = new[] { "https://fabrikamb2c.b2clogin.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/" }
                    })
                );
        }

        [Fact]
        public void Validate_FromCustomB2CAuthority_ValidateSuccessfully()
        {
            //Arrange
            string b2cAuthority = "https://myCustomDomain.com/fabrikamb2c.onmicrosoft.com/b2c_1_susi/v2.0";
            string issuer = "https://myCustomDomain.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/";
            Claim issClaim = new Claim("iss", issuer);
            Claim tfpClaim = new Claim("tfp", "b2c_1_susi");
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: issuer, claims: new[] { issClaim, tfpClaim });

            //Act
            AadIssuerValidator validator = AadIssuerValidator.GetIssuerValidator(b2cAuthority);

            //Assert
            validator.Validate(
                issuer,
                jwtSecurityToken,
                new TokenValidationParameters()
                {
                    ValidIssuers = new[] { "https://myCustomDomain.com/775527ff-9a37-4307-8b3d-cc311f58d925/v2.0/" }
                });
        }
    }
}
