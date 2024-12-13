using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Data.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly ApplicationDbContext _context;

        public PetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Pet? pet)
        {
            await _context.Pets.AddAsync(pet);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Pet?>> GetAllAsync()
        {
            return await _context.Pets.ToListAsync();
        }
        
        // Implement the GetByIdAsync method
        public async Task<Pet?> GetByIdAsync(int petId)
        {
            return await _context.Pets.FirstOrDefaultAsync(p => p != null && p.Id == petId);
        }
    }
}