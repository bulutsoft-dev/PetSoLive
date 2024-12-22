using Moq;
using PetSoLive.Core.Entities;
using PetSoLive.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace PetSoLive.Data.Tests
{
    public class UserRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly Mock<DbSet<User>> _mockUserSet;
        private readonly UserRepository _userRepository;

        public UserRepositoryTests()
        {
            // Mock the DbSet<User> and ApplicationDbContext
            _mockUserSet = new Mock<DbSet<User>>();
            _dbContextMock = new Mock<ApplicationDbContext>();
            _dbContextMock.Setup(c => c.Users).Returns(_mockUserSet.Object);

            _userRepository = new UserRepository(_dbContextMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldAddUser_WhenUserIsValid()
        {
            // Arrange
            var user = new User { Id = 1, Username = "johndoe", Email = "johndoe@example.com" };

            // Act
            await _userRepository.AddAsync(user);

            // Assert
            _mockUserSet.Verify(m => m.AddAsync(It.IsAny<User>(), default), Times.Once);
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers_WhenUsersExist()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "johndoe", Email = "johndoe@example.com" },
                new User { Id = 2, Username = "janedoe", Email = "janedoe@example.com" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<User>>();
            mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _dbContextMock.Setup(m => m.Users).Returns(mockSet.Object);

            // Act
            var result = await _userRepository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, Username = "johndoe", Email = "johndoe@example.com" };
            _dbContextMock.Setup(m => m.Users.FindAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _userRepository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("johndoe", result.Username);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            _dbContextMock.Setup(m => m.Users.FindAsync(999)).ReturnsAsync((User)null);

            // Act
            var result = await _userRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, Username = "johndoe", Email = "johndoe@example.com" };
            _dbContextMock.Setup(m => m.Users.Update(It.IsAny<User>())).Returns(Mock.Of<EntityEntry<User>>());

            // Act
            await _userRepository.UpdateAsync(user);

            // Assert
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }
    }
}
