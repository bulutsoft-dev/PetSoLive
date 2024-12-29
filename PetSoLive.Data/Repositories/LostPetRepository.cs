using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;

public class LostPetAdRepository : ILostPetAdRepository
{
    private readonly ApplicationDbContext _context;
    private ILostPetAdRepository _lostPetAdRepositoryImplementation;

    public LostPetAdRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateLostPetAdAsync(LostPetAd lostPetAd)
    {
        await _context.LostPetAds.AddAsync(lostPetAd);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<LostPetAd>> GetAllAsync()
    {
        return await _context.LostPetAds.ToListAsync();
    }

    public Task<LostPetAd> GetByIdAsync(int id)
    {
        return _lostPetAdRepositoryImplementation.GetByIdAsync(id);
    }
}