using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using PetSoLive.Infrastructure.Repositories;
using Xunit;

namespace PetSoLive.Tests.Repositories
{
    public class AdoptionRequestRepositoryTests : IDisposable
    {
        private readonly AdoptionRequestRepository _repository;
        private readonly ApplicationDbContext _context;

        public AdoptionRequestRepositoryTests()
        {
            // Her test için yeni bir GUID tabanlı veritabanı ismi kullanarak in-memory veritabanı oluşturuluyor.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Her test için benzersiz veritabanı adı
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new AdoptionRequestRepository(_context);

            // Veritabanını temizleyerek her testten önce başlatıyoruz.
            _context.Database.EnsureCreated();
        }

        // Testler bitiminde veritabanını temizliyoruz.
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetAdoptionRequestsByPetIdAsync_ShouldReturnCorrectRequests()
        {
            // Arrange
            var petId = 1;
            var userId = 1;
            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest { Id = 1, PetId = petId, UserId = userId, Status = AdoptionStatus.Pending, RequestDate = DateTime.Now },
                new AdoptionRequest { Id = 2, PetId = petId, UserId = userId, Status = AdoptionStatus.Approved, RequestDate = DateTime.Now }
            };

            // In-memory veritabanına veri ekliyoruz
            _context.AdoptionRequests.AddRange(adoptionRequests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAdoptionRequestsByPetIdAsync(petId);

            // Assert
            Assert.Equal(2, result.Count); // Beklenen 2 kayıt sayısını doğrula
        }

        [Fact]
        public async Task GetPendingRequestsByPetIdAsync_ShouldReturnOnlyPendingRequests()
        {
            // Arrange
            var petId = 1;
            var userId = 1;
            var adoptionRequests = new List<AdoptionRequest>
            {
                new AdoptionRequest { Id = 1, PetId = petId, UserId = userId, Status = AdoptionStatus.Pending, RequestDate = DateTime.Now },
                new AdoptionRequest { Id = 2, PetId = petId, UserId = userId, Status = AdoptionStatus.Approved, RequestDate = DateTime.Now }
            };

            // In-memory veritabanına veri ekliyoruz
            _context.AdoptionRequests.AddRange(adoptionRequests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingRequestsByPetIdAsync(petId);

            // Assert
            Assert.Single(result); // Yalnızca bir adet "Pending" durumunda istek olmalı
            Assert.Equal(AdoptionStatus.Pending, result.First().Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateRequestStatus()
        {
            // Arrange
            var petId = 1;
            var userId = 1;
            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = petId,
                UserId = userId,
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            // Act
            adoptionRequest.Status = AdoptionStatus.Approved;
            await _repository.UpdateAsync(adoptionRequest);

            // Assert
            var updatedRequest = await _context.AdoptionRequests.FindAsync(1);
            Assert.Equal(AdoptionStatus.Approved, updatedRequest.Status);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectRequest()
        {
            // Arrange
            var petId = 1;
            var userId = 1;
            var adoptionRequest = new AdoptionRequest
            {
                Id = 1,
                PetId = petId,
                UserId = userId,
                Status = AdoptionStatus.Pending,
                RequestDate = DateTime.Now
            };

            // Veritabanına ekliyoruz
            _context.AdoptionRequests.Add(adoptionRequest);
            await _context.SaveChangesAsync();

            // Veritabanında gerçekten kayıt var mı kontrol edelim
            var dbRequest = await _context.AdoptionRequests.FindAsync(1);
            Assert.NotNull(dbRequest); // Veritabanında kayıt var mı?

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Debug: Sonuç null mı kontrol et
            Assert.NotNull(result); // Yine de metodun doğru sonuç döndürüp döndürmediğini kontrol et
            Assert.Equal(adoptionRequest.Id, result.Id);
            Assert.Equal(AdoptionStatus.Pending, result.Status);
        }
    }
}
