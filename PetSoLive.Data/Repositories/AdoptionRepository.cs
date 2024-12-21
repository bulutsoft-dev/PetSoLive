// /PetSoLive.Data/Repositories/AdoptionRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;

namespace PetSoLive.Data.Repositories
{
    public class AdoptionRepository : IRepository<Adoption>, IAdoptionRepository
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
        
        // IsPetAlreadyAdoptedAsync yöntemini ekleyin
        public async Task<bool> IsPetAlreadyAdoptedAsync(int petId)
        {
            return await _context.Adoptions.AnyAsync(a => a.PetId == petId && a.Status == AdoptionStatus.Approved);
        }
        public async Task<Adoption?> GetAdoptionByPetIdAsync(int petId)
        {
            // Pet'in sahiplenilme bilgilerini getir
            return await _context.Adoptions
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.PetId == petId && a.Status == AdoptionStatus.Approved);
        }

        
        
        

    }
}
