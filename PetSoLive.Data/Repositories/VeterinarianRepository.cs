// Repository Implementation
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        public async Task<Veterinarian> GetApprovedByUserIdAsync(int userId)
        {
            return await _context.Veterinarians
                .FirstOrDefaultAsync(v => v.UserId == userId && v.Status == VeterinarianStatus.Approved);
        }

        public async Task<Veterinarian> CreateAsync(Veterinarian veterinarian)
        {
            _context.Veterinarians.Add(veterinarian);
            await _context.SaveChangesAsync();
            return veterinarian;
        }

        public async Task UpdateAsync(Veterinarian veterinarian)
        {
            // AppliedDate'in UTC olduğundan emin ol
            if (veterinarian.AppliedDate.Kind != DateTimeKind.Utc)
            {
                veterinarian.AppliedDate = DateTime.SpecifyKind(veterinarian.AppliedDate, DateTimeKind.Utc);
            }

            _context.Veterinarians.Update(veterinarian);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Veterinarian>> GetAllAsync()
        {
            return await _context.Veterinarians
                .Include(v => v.User)  // Ensure that User is included in the query
                .ToListAsync();
        }

        public async Task<Veterinarian> GetByIdAsync(int id)
        {
            return await _context.Veterinarians.FindAsync(id);
        }

        public async Task<List<Veterinarian>> GetAllVeterinariansAsync()
        {
            return await _context.Veterinarians
                .Where(v => v.Status == VeterinarianStatus.Approved)
                .ToListAsync();
        }
    }
}