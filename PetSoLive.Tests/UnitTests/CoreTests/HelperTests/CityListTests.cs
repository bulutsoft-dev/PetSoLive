using System.Collections.Generic;
using Xunit;
namespace PetSoLive.Tests.UnitTests.CoreTests.HelperTests;

public class CityListTests
{
    [Fact]
    public void Cities_ShouldContainExpectedFourCities()
    {
        // Arrange & Act
        var cities = CityList.Cities;

        // Assert
        Assert.NotNull(cities);
        Assert.Equal(4, cities.Count);
        Assert.Contains("Manisa", cities);
        Assert.Contains("İzmir", cities);
        Assert.Contains("İstanbul", cities);
        Assert.Contains("Muğla", cities);
    }

    [Theory]
    [InlineData("İstanbul", new[] { "Kadıköy", "Beşiktaş", "Üsküdar" })]
    [InlineData("Manisa",   new[] { "Akhisar", "Turgutlu", "Salihli" })]
    [InlineData("İzmir",    new[] { "Karşıyaka", "Konak", "Bornova" })]
    [InlineData("Muğla",    new[] { "Menteşe", "Bodrum", "Fethiye", "Milas" })]
    public void GetDistrictsByCity_WhenCityIsKnown_ReturnsCorrectDistricts(string city, string[] expectedDistricts)
    {
        // Arrange & Act
        var actualDistricts = CityList.GetDistrictsByCity(city);

        // Assert
        Assert.NotNull(actualDistricts);
        Assert.Equal(expectedDistricts.Length, actualDistricts.Count);

        foreach (var district in expectedDistricts)
        {
            Assert.Contains(district, actualDistricts);
        }
    }

    [Fact]
    public void GetDistrictsByCity_WhenCityIsUnknown_ReturnsEmptyList()
    {
        // Arrange
        var city = "UnknownCity";

        // Act
        var districts = CityList.GetDistrictsByCity(city);

        // Assert
        Assert.NotNull(districts);
        Assert.Empty(districts);
    }
}