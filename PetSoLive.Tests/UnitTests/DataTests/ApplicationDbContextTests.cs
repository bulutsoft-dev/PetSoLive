using PetSoLive.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PetSoLive.Data.Tests
{
    public class ApplicationDbContextTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ApplicationDbContextTests()
        {
            // Configure the in-memory database for testing
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PetSoLiveTestDb")
                .Options;
        }

        [Fact]
        public async Task OnModelCreating_ShouldConfigurePetOwnerCompositeKey()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            // Act
            var petOwner = new PetOwner
            {
                PetId = 1,
                UserId = 1
            };
            context.PetOwners.Add(petOwner);
            await context.SaveChangesAsync();

            // Assert
            var savedPetOwner = await context.PetOwners.FirstOrDefaultAsync(po => po.PetId == 1 && po.UserId == 1);
            Assert.NotNull(savedPetOwner);
        }

        [Fact]
        public async Task OnModelCreating_ShouldEstablishCorrectRelationships()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
            var pet = new Pet { Id = 1, Name = "Buddy" };

            context.Users.Add(user);
            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            var petOwner = new PetOwner { PetId = pet.Id, UserId = user.Id };
            context.PetOwners.Add(petOwner);
            await context.SaveChangesAsync();

            // Act
            var savedPetOwner = await context.PetOwners
                .Where(po => po.UserId == 1 && po.PetId == 1)
                .Include(po => po.User)
                .Include(po => po.Pet)
                .FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(savedPetOwner);
            Assert.Equal(user.Id, savedPetOwner.UserId);
            Assert.Equal(pet.Id, savedPetOwner.PetId);
        }

        [Fact]
        public async Task ShouldHaveAdoptionRelationships()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
            var pet = new Pet { Id = 1, Name = "Buddy" };
            var adoption = new Adoption { UserId = 1, PetId = 1, AdoptionDate = DateTime.Now };

            context.Users.Add(user);
            context.Pets.Add(pet);
            context.Adoptions.Add(adoption);
            await context.SaveChangesAsync();

            // Act
            var savedAdoption = await context.Adoptions
                .Where(a => a.UserId == 1 && a.PetId == 1)
                .Include(a => a.User)
                .Include(a => a.Pet)
                .FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(savedAdoption);
            Assert.Equal(user.Id, savedAdoption.UserId);
            Assert.Equal(pet.Id, savedAdoption.PetId);
        }

        [Fact]
        public async Task ShouldAddAdoptionRequestWithRelationships()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
            var pet = new Pet { Id = 1, Name = "Buddy" };
            var adoptionRequest = new AdoptionRequest { UserId = 1, PetId = 1, RequestDate = DateTime.Now };

            context.Users.Add(user);
            context.Pets.Add(pet);
            context.AdoptionRequests.Add(adoptionRequest);
            await context.SaveChangesAsync();

            // Act
            var savedAdoptionRequest = await context.AdoptionRequests
                .Where(ar => ar.UserId == 1 && ar.PetId == 1)
                .Include(ar => ar.User)
                .Include(ar => ar.Pet)
                .FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(savedAdoptionRequest);
            Assert.Equal(user.Id, savedAdoptionRequest.UserId);
            Assert.Equal(pet.Id, savedAdoptionRequest.PetId);
        }
    }
}
