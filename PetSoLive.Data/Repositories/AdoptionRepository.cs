// /PetSoLive.Data/Repositories/AdoptionRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

namespace PetSoLive.Data.Repositories
{
    public class AdoptionRepository : IRepository<Adoption>
    {
        private readonly ApplicationDbContext _context;

        // Constructor to inject the ApplicationDbContext
        public AdoptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new Adoption entity asynchronously
        public async Task AddAsync(Adoption entity)
        {
            await _context.Adoptions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Get all Adoptions with related Pet and User entities
        public async Task<IEnumerable<Adoption>> GetAllAsync()
        {
            return await _context.Adoptions
                .Include(a => a.Pet) // Eagerly load Pet data
                .Include(a => a.User) // Eagerly load User data
                .ToListAsync();
        }
    }
}
