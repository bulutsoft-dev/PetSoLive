using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetSoLive.Core.Enums;
using PetSoLive.Data;

namespace PetSoLive.Infrastructure.Repositories
{
    public class AdoptionRequestRepository : IAdoptionRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public AdoptionRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId)
        {
            return await _context.AdoptionRequests
                .Where(ar => ar.PetId == petId)
                .Include(ar => ar.User)
                .ToListAsync();
        }

        public async Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId)
        {
            return await _context.AdoptionRequests
                .Where(r => r.PetId == petId && r.Status == AdoptionStatus.Pending)
                .ToListAsync();
        }

        public async Task<AdoptionRequest> GetByIdAsync(int adoptionRequestId)
        {
            return await _context.AdoptionRequests
                .Include(r => r.Pet)
                .FirstOrDefaultAsync(r => r.Id == adoptionRequestId);
        }

        public async Task UpdateAsync(AdoptionRequest adoptionRequest)
        {
            _context.AdoptionRequests.Update(adoptionRequest);
            await _context.SaveChangesAsync();
        }
    }
}