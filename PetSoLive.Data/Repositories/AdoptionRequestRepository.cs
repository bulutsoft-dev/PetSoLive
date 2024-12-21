using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Data.Repositories
{
    public class AdoptionRequestRepository : IAdoptionRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public AdoptionRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AdoptionRequest adoptionRequest)
        {
            await _context.AdoptionRequests.AddAsync(adoptionRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<AdoptionRequest?> GetAdoptionRequestByPetIdAsync(int petId)
        {
            return await _context.AdoptionRequests
                .FirstOrDefaultAsync(ar => ar.PetId == petId);
        }

        public async Task<AdoptionRequest?> GetByIdAsync(int id)
        {
            return await _context.AdoptionRequests
                .FirstOrDefaultAsync(ar => ar.Id == id);
        }

        public async Task UpdateAsync(AdoptionRequest adoptionRequest)
        {
            _context.AdoptionRequests.Update(adoptionRequest);
            await _context.SaveChangesAsync();
        }
    }
}