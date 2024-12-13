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

        // Delete an Adoption entity by its ID asynchronously
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Adoptions.FindAsync(id);
            if (entity != null)
            {
                _context.Adoptions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Get all Adoptions with related Pet and User entities
        public async Task<IEnumerable<Adoption>> GetAllAsync()
        {
            return await _context.Adoptions
                .Include(a => a.Pet) // Eagerly load Pet data
                .Include(a => a.User) // Eagerly load User data
                .ToListAsync();
        }

        // Get a single Adoption by its ID with related Pet and User entities
        public async Task<Adoption> GetByIdAsync(int id)
        {
            return await _context.Adoptions
                .Include(a => a.Pet) // Eagerly load Pet data
                .Include(a => a.User) // Eagerly load User data
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Update an existing Adoption entity asynchronously
        public async Task UpdateAsync(Adoption entity)
        {
            _context.Adoptions.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
