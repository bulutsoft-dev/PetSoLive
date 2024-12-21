using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using System.Threading.Tasks;

namespace PetSoLive.Data.Repositories
{
    public class PetOwnerRepository : IPetOwnerRepository
    {
        private readonly ApplicationDbContext _context;

        public PetOwnerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PetOwner petOwner)
        {
            await _context.PetOwners.AddAsync(petOwner);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}