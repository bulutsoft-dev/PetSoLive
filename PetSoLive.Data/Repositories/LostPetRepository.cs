using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using PetSoLive.Core.Entities;
using PetSoLive.Core.DTOs;
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

    public async Task<IEnumerable<LostPetAd>> GetAllAsync()
    {
        return await _context.LostPetAds.ToListAsync();
    }

    public async Task<LostPetAd> GetByIdAsync(int id)
    {
        return await _context.LostPetAds
            .FirstOrDefaultAsync(ad => ad.Id == id);
    }

    public async Task UpdateLostPetAdAsync(LostPetAd lostPetAd)
    {
        _context.LostPetAds.Update(lostPetAd);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLostPetAdAsync(LostPetAd lostPetAd)
    {
        _context.LostPetAds.Remove(lostPetAd);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<LostPetAd>> GetFilteredAsync(LostPetAdFilterDto filterDto)
    {
        var filter = filterDto;
        var query = _context.LostPetAds.AsQueryable();
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(x => x.LastSeenCity == filter.City);
            if (!string.IsNullOrEmpty(filter.District))
                query = query.Where(x => x.LastSeenDistrict == filter.District);
            if (!string.IsNullOrEmpty(filter.PetType))
                query = query.Where(x => x.PetName.Contains(filter.PetType));
            if (filter.DatePostedAfter.HasValue)
                query = query.Where(x => x.CreatedAt >= filter.DatePostedAfter.Value);
        }
        return await query.ToListAsync();
    }
}