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
    public class VeterinarianRepository : IVeterinarianRepository
    {
        private readonly ApplicationDbContext _context;

        public VeterinarianRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Veterinarian> GetByUserIdAsync(int userId)
        {
            return await _context.Veterinarians.FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public async Task<Veterinarian> CreateAsync(Veterinarian veterinarian)
        {
            _context.Veterinarians.Add(veterinarian);
            await _context.SaveChangesAsync();
            return veterinarian;
        }

        public async Task UpdateAsync(Veterinarian veterinarian)
        {
            _context.Veterinarians.Update(veterinarian);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Veterinarian>> GetAllAsync()
        {
            return await _context.Veterinarians
                .Include(v => v.User)  // Ensure that User is included in the query
                .ToListAsync();
        }


        public async Task<Veterinarian> GetByIdAsync(int id)  // Add this method
        {
            return await _context.Veterinarians.FindAsync(id); // Retrieve veterinarian by Id
        }
        
        public async Task<List<Veterinarian>> GetAllVeterinariansAsync()
        {
            return await _context.Veterinarians
                .Where(v => v.Status == VeterinarianStatus.Approved) // Onaylı veterinerleri filtrele
                .ToListAsync();
        }
    }
}