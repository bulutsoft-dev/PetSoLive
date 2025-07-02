using Xunit;
using Petsolive.API.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PetSoLive.Tests.UnitTests.APITests.Helpers
{
    public class JwtHelperTests
    {
        public JwtHelperTests()
        {
            // Set required environment variables for JwtHelper
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "TestSecretKey12345678901234567890123456789012");
            Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
            Environment.SetEnvironmentVariable("JWT_EXPIRE_MINUTES", "60");
        }

        [Fact]
        public void GenerateToken_ReturnsNonEmptyString_ForValidInput()
        {
            // Arrange
            var helper = new JwtHelper();
            int userId = 42;
            string username = "testuser";
            var roles = new List<string> { "Admin", "User" };

            // Act
            var token = helper.GenerateToken(userId, username, roles);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerateToken_ContainsCorrectClaims()
        {
            // Arrange
            var helper = new JwtHelper();
            int userId = 99;
            string username = "jwtuser";
            var roles = new List<string> { "User", "Moderator" };

            // Act
            var token = helper.GenerateToken(userId, username, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            Assert.Equal(userId.ToString(), jwt.Subject);
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == username);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Moderator");
            Assert.Equal("TestIssuer", jwt.Issuer);
            Assert.Equal("TestAudience", jwt.Audiences.FirstOrDefault());
        }

        [Fact]
        public void Constructor_ThrowsException_IfSecretKeyNotSet()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => new JwtHelper());
            Assert.Contains("JWT_SECRET_KEY not set", ex.Message);

            // Reset for other tests
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "TestSecretKey12345678901234567890123456789012");
        }
    }
} 