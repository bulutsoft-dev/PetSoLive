using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Business.Services;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;
namespace PetSoLive.Tests.UnitTests;

public class LostPetAdServiceTests
{
        private readonly Mock<ILostPetAdRepository> _lostPetAdRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;

        // System Under Test (SUT)
        private readonly LostPetAdService _lostPetAdService;

        public LostPetAdServiceTests()
        {
            _lostPetAdRepositoryMock = new Mock<ILostPetAdRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();

            // Initialize SUT with mocks
            _lostPetAdService = new LostPetAdService(
                _lostPetAdRepositoryMock.Object,
                _userRepositoryMock.Object,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateLostPetAdAsync_WhenCalled_SetsCityAndDistrictAndCreatesLostPetAd()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                Id = 1,
                UserId = 123,
            };
            var city = "TestCity";
            var district = "TestDistrict";
            var user = new User
            {
                Id = 123,
                Email = "user123@test.com"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(lostPetAd.UserId))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.GetUsersByLocationAsync(city, district))
                .ReturnsAsync(new List<User>()); // empty list for simplicity

            // Act
            await _lostPetAdService.CreateLostPetAdAsync(lostPetAd, city, district);

            // Assert
            Assert.Equal(city, lostPetAd.LastSeenCity);
            Assert.Equal(district, lostPetAd.LastSeenDistrict);

            _lostPetAdRepositoryMock
                .Verify(x => x.CreateLostPetAdAsync(It.Is<LostPetAd>(
                    ad => ad.LastSeenCity == city && ad.LastSeenDistrict == district
                )), Times.Once);
        }

        [Fact]
        public async Task CreateLostPetAdAsync_WhenUserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                Id = 1,
                UserId = 999, // Non-existent user
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(lostPetAd.UserId))
                .ReturnsAsync((User)null); // user not found

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _lostPetAdService.CreateLostPetAdAsync(lostPetAd, "City", "District"));
        }

        [Fact]
        public async Task CreateLostPetAdAsync_WhenCalled_SendsEmailsToUsersInLocation()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                Id = 2,
                UserId = 123,
            };
            var city = "TestCity";
            var district = "TestDistrict";
            var user = new User
            {
                Id = 123,
                Email = "owner@test.com"
            };

            var usersInLocation = new List<User>
            {
                new User { Id = 100, Email = "test1@test.com" },
                new User { Id = 101, Email = "test2@test.com" }
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(lostPetAd.UserId))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.GetUsersByLocationAsync(city, district))
                .ReturnsAsync(usersInLocation);

            // Act
            await _lostPetAdService.CreateLostPetAdAsync(lostPetAd, city, district);

            // Assert
            // Verify CreateLostPetAdAsync was called
            _lostPetAdRepositoryMock.Verify(
                x => x.CreateLostPetAdAsync(It.IsAny<LostPetAd>()),
                Times.Once
            );

            // Verify SendEmailAsync was called for each user in location
            _emailServiceMock.Verify(
                x => x.SendEmailAsync(
                    "test1@test.com",
                    "New Lost Pet Ad Created",
                    It.IsAny<string>()),
                Times.Once
            );

            _emailServiceMock.Verify(
                x => x.SendEmailAsync(
                    "test2@test.com",
                    "New Lost Pet Ad Created",
                    It.IsAny<string>()),
                Times.Once
            );
        }

        

        [Fact]
        public async Task GetLostPetAdByIdAsync_WhenCalled_ReturnsLostPetAd()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                Id = 10,
                UserId = 999
            };

            _lostPetAdRepositoryMock
                .Setup(x => x.GetByIdAsync(lostPetAd.Id))
                .ReturnsAsync(lostPetAd);

            // Act
            var result = await _lostPetAdService.GetLostPetAdByIdAsync(lostPetAd.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(lostPetAd.Id, result.Id);
        }

        [Fact]
        public async Task GetLostPetAdByIdAsync_WhenLostPetAdExists_FetchesAndAttachesUser()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                Id = 10,
                UserId = 999
            };
            var user = new User
            {
                Id = 999,
                Email = "test@user.com"
            };

            _lostPetAdRepositoryMock
                .Setup(x => x.GetByIdAsync(lostPetAd.Id))
                .ReturnsAsync(lostPetAd);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(lostPetAd.UserId))
                .ReturnsAsync(user);

            // Act
            var result = await _lostPetAdService.GetLostPetAdByIdAsync(lostPetAd.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Equal(999, result.User.Id);
            Assert.Equal("test@user.com", result.User.Email);
        }

        [Fact]
        public async Task UpdateLostPetAdAsync_WhenCalled_UpdatesLostPetAd()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                Id = 1,
            };

            _lostPetAdRepositoryMock
                .Setup(x => x.UpdateLostPetAdAsync(lostPetAd))
                .Returns(Task.CompletedTask);

            // Act
            await _lostPetAdService.UpdateLostPetAdAsync(lostPetAd);
            
        }

        [Fact]
        public async Task DeleteLostPetAdAsync_WhenCalled_DeletesLostPetAd()
        {
            // Arrange
            var lostPetAd = new LostPetAd
            {
                Id = 3,
            };

            _lostPetAdRepositoryMock
                .Setup(x => x.DeleteLostPetAdAsync(lostPetAd))
                .Returns(Task.CompletedTask);

            // Act
            await _lostPetAdService.DeleteLostPetAdAsync(lostPetAd);

            // Assert
            _lostPetAdRepositoryMock.Verify(
                x => x.DeleteLostPetAdAsync(It.Is<LostPetAd>(ad => ad.Id == 3)),
                Times.Once
            );
        }
    }
