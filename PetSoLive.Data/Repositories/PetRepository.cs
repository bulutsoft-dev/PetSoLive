using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;

public class PetRepository : IPetRepository
{
    private readonly ApplicationDbContext _context;

    public PetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Pet? pet)
    {
        if (pet == null) throw new ArgumentNullException(nameof(pet));
        await _context.Pets.AddAsync(pet);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Pet?>> GetAllAsync()
    {
        return await _context.Pets.ToListAsync();
    }

    public async Task<Pet?> GetByIdAsync(int petId)
    {
        return await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId);
    }

    public async Task<List<PetOwner>> GetPetOwnersAsync(int petId)
    {
        return await _context.PetOwners
            .Where(po => po.PetId == petId)
            .ToListAsync();
    }

    public async Task UpdateAsync(Pet existingPet)
    {
        _context.Pets.Update(existingPet);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Pet pet)
    {
        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();
    }
}