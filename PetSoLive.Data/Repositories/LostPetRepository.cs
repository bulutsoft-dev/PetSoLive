using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Data.Repositories;

public class LostPetAdRepository : ILostPetAdRepository
{
    private readonly ApplicationDbContext _context;

    public LostPetAdRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateLostPetAdAsync(LostPetAd lostPetAd)
    {
        await _context.LostPetAds.AddAsync(lostPetAd);
        await _context.SaveChangesAsync();
    }

    public async Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location)
    {
        return await _context.LostPetAds
            .Where(ad => ad.LastSeenLocation.Contains(location))
            .ToListAsync();
    }
}
