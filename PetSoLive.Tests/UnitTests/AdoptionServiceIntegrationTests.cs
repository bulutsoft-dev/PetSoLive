using Microsoft.Extensions.DependencyInjection;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Threading.Tasks;
using PetSoLive.Data;
using Xunit;

namespace PetSoLive.Tests.UnitTests
{
    public class AdoptionServiceIntegrationTests
    {
        private readonly ServiceProvider _serviceProvider;

        public AdoptionServiceIntegrationTests()
        {
            _serviceProvider = TestHelper.CreateServiceProvider();
        }

        [Fact]
        public async Task CreateAdoption_ShouldSucceed_WhenPetNotAdopted()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var adoptionService = scope.ServiceProvider.GetRequiredService<IAdoptionService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pet = new Pet { Id = 1, Name = "Fluffy", Species = "Cat", Breed = "Siamese" };
            var user = new User { Id = 1, Username = "TestUser" };

            dbContext.Pets.Add(pet);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var adoption = new Adoption
            {
                PetId = pet.Id,
                UserId = user.Id,
                AdoptionDate = DateTime.Now,
                Status = Core.Enums.AdoptionStatus.Pending
            };

            // Act
            await adoptionService.CreateAdoptionAsync(adoption);

            // Assert
            var savedAdoption = await dbContext.Adoptions.FindAsync(adoption.Id);
            Assert.NotNull(savedAdoption);
            Assert.Equal(adoption.PetId, savedAdoption.PetId);
        }
    }
}