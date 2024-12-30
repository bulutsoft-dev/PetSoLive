using System;
using Xunit;
using PetSoLive.Core.Enums; 

namespace PetSoLive.Tests.UnitTests.EnumTests;

public class EmergencyLevelTests
{

        [Fact]
        public void EmergencyLevel_Values_ShouldMatchExpectedIntegers()
        {
            // Verify numeric ordering:
            // Low = 0, Medium = 1, High = 2 (by default, unless changed)
            Assert.Equal(0, (int)EmergencyLevel.Low);
            Assert.Equal(1, (int)EmergencyLevel.Medium);
            Assert.Equal(2, (int)EmergencyLevel.High);
        }

        [Theory]
        [InlineData("Low", EmergencyLevel.Low)]
        [InlineData("Medium", EmergencyLevel.Medium)]
        [InlineData("High", EmergencyLevel.High)]
        public void EmergencyLevel_ParseFromString_ReturnsCorrectEnum(string input, EmergencyLevel expected)
        {
            // Act
            var parseSuccess = Enum.TryParse<EmergencyLevel>(input, out var result);

            // Assert
            Assert.True(parseSuccess);
            Assert.Equal(expected, result);
        }

        
}