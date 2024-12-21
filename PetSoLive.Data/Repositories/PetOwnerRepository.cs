// PetOwnerRepository.cs

using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;

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

// PetOwnerRepository.cs
    public async Task<PetOwner> GetPetOwnerByPetIdAsync(int petId)
    {
        return await _context.PetOwners
            .Include(po => po.User)  // Include the User navigation property
            .FirstOrDefaultAsync(po => po.PetId == petId);
    }
    
    

}