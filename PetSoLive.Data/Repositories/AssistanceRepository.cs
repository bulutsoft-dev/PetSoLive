using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

namespace PetSoLive.Data.Repositories
{
    public class AssistanceRepository : IRepository<Assistance>, IAsyncDisposable
    {
        private readonly ApplicationDbContext _context;

        public AssistanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new Assistance
        public async Task AddAsync(Assistance entity)
        {
            await _context.Assistances.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Get all Assistances
        public async Task<IEnumerable<Assistance>> GetAllAsync()
        {
            return await _context.Assistances.ToListAsync();
        }

        // Get Assistance by ID
        public async Task<Assistance?> GetByIdAsync(int id)
        {
            return await _context.Assistances.FindAsync(id);
        }

        // Update an Assistance (if required)
        public async Task UpdateAsync(Assistance entity)
        {
            _context.Assistances.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Delete an Assistance (if required)
        public async Task DeleteAsync(Assistance entity)
        {
            _context.Assistances.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // Implementing IAsyncDisposable to clean up resources
        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}