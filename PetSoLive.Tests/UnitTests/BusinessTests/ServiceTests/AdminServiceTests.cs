using Moq;
using System.Threading.Tasks;
using Xunit;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Tests.UnitTests;

public class AdminServiceTests
{
    private readonly Mock<IAdminRepository> _adminRepositoryMock;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _adminRepositoryMock = new Mock<IAdminRepository>();
        _adminService = new AdminService(_adminRepositoryMock.Object);
    }

    [Fact]
    public async Task IsUserAdminAsync_WhenUserIsAdmin_ReturnsTrue()
    {
        // Arrange
        var userId = 123;
        _adminRepositoryMock
            .Setup(repo => repo.IsUserAdminAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _adminService.IsUserAdminAsync(userId);

        // Assert
        Assert.True(result);
        _adminRepositoryMock.Verify(
            repo => repo.IsUserAdminAsync(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task IsUserAdminAsync_WhenUserIsNotAdmin_ReturnsFalse()
    {
        // Arrange
        var userId = 456;
        _adminRepositoryMock
            .Setup(repo => repo.IsUserAdminAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _adminService.IsUserAdminAsync(userId);

        // Assert
        Assert.False(result);
        _adminRepositoryMock.Verify(
            repo => repo.IsUserAdminAsync(userId),
            Times.Once
        );
    }
}