using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            // Fetch all adoption requests related to a specific pet
            return await _context.AdoptionRequests
                .Where(ar => ar.PetId == petId)
                .Include(ar => ar.User)  // Include User details to display in the list
                .ToListAsync();
        }
    }
}