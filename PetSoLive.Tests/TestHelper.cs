using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using PetSoLive.Data.Repositories;

namespace PetSoLive.Tests
{
    public static class TestHelper
    {
        public static ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("PetSoLiveTestDb") // In-memory database
                .Options;

            return new ApplicationDbContext(options);
        }

        public static ServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            // Add InMemory DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("PetSoLiveTestDb"));

            // Add repositories and services
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<Adoption>, AdoptionRepository>();
            services.AddScoped<IAdoptionService, AdoptionService>();

            return services.BuildServiceProvider();
        }
    }
}