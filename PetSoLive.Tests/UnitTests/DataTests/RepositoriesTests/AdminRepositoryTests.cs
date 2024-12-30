using Microsoft.EntityFrameworkCore;
using Xunit;
using PetSoLive.Core.Entities;    // Where Admin entity resides
using PetSoLive.Data;             // Where ApplicationDbContext resides

namespace PetSoLive.Tests.UnitTests.DataTests.RepositoriesTests;

public class AdminRepositoryTests
{
            /// <summary>
        /// A minimal DbContext derived from ApplicationDbContext for in-memory testing.
        /// If your ApplicationDbContext requires a constructor with options, adjust as needed.
        /// </summary>
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB name per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task IsUserAdminAsync_WhenUserIsAdmin_ReturnsTrue()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            var repository = new AdminRepository(context);

            // Seed the database with an Admin record for userId=123
            context.Admins.Add(new Admin
            {
                Id = 1,
                UserId = 123,
                CreatedDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // Act
            var isAdmin = await repository.IsUserAdminAsync(123);

            // Assert
            Assert.True(isAdmin, "Expected user 123 to be recognized as admin.");
        }

        [Fact]
        public async Task IsUserAdminAsync_WhenUserIsNotAdmin_ReturnsFalse()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            var repository = new AdminRepository(context);

            // Notice we don't add any Admin record for userId=456
            // so the user is effectively "not admin"

            // Act
            var isAdmin = await repository.IsUserAdminAsync(456);

            // Assert
            Assert.False(isAdmin, "Expected user 456 to not be recognized as admin.");
        }

        [Fact]
        public async Task IsUserAdminAsync_WhenNoAdminsExist_ReturnsFalse()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            var repository = new AdminRepository(context);

            // Database is empty (no Admin records at all)

            // Act
            var isAdmin = await repository.IsUserAdminAsync(999);

            // Assert
            Assert.False(isAdmin);
        }
}