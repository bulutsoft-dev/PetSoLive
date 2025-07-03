using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Data;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Data
{
    public class AdoptionRepository : IAdoptionRepository
    {
        private readonly ApplicationDbContext _context;

        public AdoptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Adoption adoption)
        {
            await _context.Adoptions.AddAsync(adoption);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(AdoptionRequest adoptionRequest)
        {
            await _context.AdoptionRequests.AddAsync(adoptionRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<Adoption?> GetAdoptionByPetIdAsync(int petId)
        {
            return await _context.Adoptions
                .FirstOrDefaultAsync(a => a.PetId == petId);
        }

        public async Task<bool> IsPetAlreadyAdoptedAsync(int petId)
        {
            return await _context.Adoptions
                .AnyAsync(a => a.PetId == petId && a.Status == AdoptionStatus.Approved);
        }

        public async Task<bool> HasUserAlreadyRequestedAdoptionAsync(int userId, int petId)
        {
            return await _context.AdoptionRequests
                .AnyAsync(ar => ar.UserId == userId && ar.PetId == petId);
        }

        public async Task<AdoptionRequest?> GetAdoptionRequestByUserAndPetAsync(int userId, int petId)
        {
            return await _context.AdoptionRequests
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.PetId == petId);
        }

        public async Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId)
        {
            return await _context.AdoptionRequests
                .Where(ar => ar.PetId == petId && ar.Status == AdoptionStatus.Pending)
                .ToListAsync();
        }

        public async Task<Adoption?> GetAdoptionByPetAndUserAsync(int petId, int userId)
        {
            return await _context.Adoptions
                .FirstOrDefaultAsync(a => a.PetId == petId && a.UserId == userId);
        }
    }
}