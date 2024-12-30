using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using PetSoLive.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PetSoLive.Tests.UnitTests.DataTests.RepositoriesTests;

public class HelpRequestRepositoryTests
{
     private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB name per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateHelpRequestAsync_WhenCalled_AddsHelpRequestToDatabase()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IHelpRequestRepository repository = new HelpRequestRepository(context);

            var helpRequest = new HelpRequest
            {
                Id = 1,
                Title = "Need help with cat",
                Description = "A stray cat is injured and needs medical attention.",
                CreatedAt = DateTime.UtcNow,
                UserId = 10,
                Location = "123 Cat Street",
            };

            // Act
            await repository.CreateHelpRequestAsync(helpRequest);

            // Assert
            var inserted = await context.HelpRequests.FindAsync(1);
            Assert.NotNull(inserted);
            Assert.Equal("Need help with cat", inserted.Title);
            Assert.Equal(10, inserted.UserId);
        }
        

        [Fact]
        public async Task UpdateHelpRequestAsync_WhenCalled_UpdatesExistingHelpRequest()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IHelpRequestRepository repository = new HelpRequestRepository(context);

            var hr = new HelpRequest
            {
                Id = 2,
                Title = "Initial Title",
                Description = "Initial Description",
                CreatedAt = DateTime.UtcNow,
                Location = "123 Cat Street",
            };
            context.HelpRequests.Add(hr);
            await context.SaveChangesAsync();

            // Act
            hr.Title = "Updated Title";
            hr.Description = "Updated Description";
            await repository.UpdateHelpRequestAsync(hr);

            // Assert
            var updatedHr = await context.HelpRequests.FindAsync(2);
            Assert.NotNull(updatedHr);
            Assert.Equal("Updated Title", updatedHr.Title);
            Assert.Equal("Updated Description", updatedHr.Description);
        }

        [Fact]
        public async Task UpdateHelpRequestAsync_WhenHelpRequestNotFound_DoesNothing()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IHelpRequestRepository repository = new HelpRequestRepository(context);

            // A help request with Id=999 that is not in the DB
            var missingHr = new HelpRequest
            {
                Id = 999,
                Title = "Not in DB"
            };

            // Act
            await repository.UpdateHelpRequestAsync(missingHr);

            // Assert
            var result = await context.HelpRequests.FindAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteHelpRequestAsync_WhenCalled_RemovesRequestIfItExists()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IHelpRequestRepository repository = new HelpRequestRepository(context);

            var hr = new HelpRequest
            {
                Id = 3,
                Title = "To be deleted",
                Description = "",
                Location = ""
            };
            context.HelpRequests.Add(hr);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteHelpRequestAsync(hr.Id);

            // Assert
            var deleted = await context.HelpRequests.FindAsync(3);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteHelpRequestAsync_WhenRequestNotFound_DoesNothing()
        {
            // Arrange
            await using var context = GetInMemoryContext();
            IHelpRequestRepository repository = new HelpRequestRepository(context);

            // No record with Id=999 in the DB
            // Act
            await repository.DeleteHelpRequestAsync(999);

            // Assert
            // No exception should occur; simply does nothing
            Assert.True(true);
        }
}