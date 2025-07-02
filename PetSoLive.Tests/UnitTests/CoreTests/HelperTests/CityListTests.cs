using System.Collections.Generic;
using Xunit;
namespace PetSoLive.Tests.UnitTests.CoreTests.HelperTests;

public class CityListTests
{
    [Fact]
    public void Cities_ShouldContainExpectedCities()
    {
        // Arrange & Act
        var cities = CityList.Cities;

        // Assert
        Assert.NotNull(cities);
        Assert.True(cities.Count >= 81); // Türkiye'deki şehir sayısı
        Assert.Contains("Manisa", cities);
        Assert.Contains("İzmir", cities);
        Assert.Contains("İstanbul", cities);
        Assert.Contains("Muğla", cities);
    }

    [Theory]
    [InlineData("İstanbul", new[] { "Kadıköy", "Beşiktaş", "Üsküdar", "Fatih" })]
    [InlineData("Manisa",   new[] { "Akhisar", "Turgutlu", "Salihli", "Alaşehir" })]
    [InlineData("İzmir",    new[] { "Karşıyaka", "Konak", "Bornova", "Buca" })]
    [InlineData("Muğla",    new[] { "Menteşe", "Bodrum", "Fethiye", "Marmaris" })]
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
    public void GetDistrictsByCity_WhenCityIsUnknown_ReturnsOther()
    {
        // Arrange
        var city = "UnknownCity";

        // Act
        var districts = CityList.GetDistrictsByCity(city);

        // Assert
        Assert.NotNull(districts);
        Assert.Single(districts);
        Assert.Equal("Diğer", districts[0]);
    }
}