using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> _mockUserRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IRepository<User>>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnUser_WhenCredentialsAreValid()
        {
            // Arrange
            var username = "testuser";
            var password = "validPassword";
            var user = new User
            {
                Id = 1,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                LastLoginDate = null
            };
            _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.AuthenticateAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.LastLoginDate != null)), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
        {
            // Arrange
            var username = "testuser";
            var password = "invalidPassword";
            var user = new User
            {
                Id = 1,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("validPassword"),
                LastLoginDate = null
            };
            _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.AuthenticateAsync(username, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenUsernameOrEmailExists()
        {
            // Arrange
            var user = new User { Username = "existinguser", Email = "existing@example.com", PasswordHash = "password" };
            _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { user });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.RegisterAsync(user));
        }

        [Fact]
        public async Task RegisterAsync_ShouldAddUser_WhenValid()
        {
            // Arrange
            var user = new User
            {
                Username = "newuser",
                Email = "new@example.com",
                PasswordHash = "password"
            };
            _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());

            // Act
            await _userService.RegisterAsync(user);

            // Assert
            _mockUserRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == user.Username && u.Email == user.Email)), Times.Once);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenFound()
        {
            // Arrange
            var username = "testuser";
            var user = new User { Id = 1, Username = username };
            _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var username = "testuser";
            _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenFound()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Username = "testuser" };
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldThrowKeyNotFoundException_WhenNotFound()
        {
            // Arrange
            var userId = 1;
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null); // Simulate that user is not found

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetUserByIdAsync(userId));
            Assert.Equal("User not found.", exception.Message);
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldThrowArgumentNullException_WhenUserIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUserAsync(null));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser" };
            _mockUserRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.UpdateUserAsync(user));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenValidChanges()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
            var updatedUser = new User { Id = 1, Username = "newuser", Email = "new@example.com" };

            _mockUserRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            // Act
            await _userService.UpdateUserAsync(updatedUser);

            // Assert
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Username == "newuser" && u.Email == "new@example.com")), Times.Once);
        }
    }
}
