using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;

namespace PetSoLive.Business.Services
{
    public class AdoptionRequestService : IAdoptionRequestService
    {
        private readonly ApplicationDbContext _context;

        public AdoptionRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Pet> GetPetByIdAsync(int petId)
        {
            return await _context.Pets
                .FirstOrDefaultAsync(p => p.Id == petId);
        }

        public async Task<AdoptionRequest> GetAdoptionRequestByIdAsync(int requestId)
        {
            return await _context.AdoptionRequests
                .Include(ar => ar.User)
                .FirstOrDefaultAsync(ar => ar.Id == requestId);
        }

        public async Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId)
        {
            return await _context.AdoptionRequests
                .Where(ar => ar.PetId == petId)
                .ToListAsync();
        }

        public async Task UpdateAdoptionRequestAsync(AdoptionRequest request)
        {
            _context.AdoptionRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePetAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
        }
    }
}