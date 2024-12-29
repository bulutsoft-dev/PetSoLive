using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using PetSoLive.Data;
using System.Linq;
using System.Threading.Tasks;

public class LostPetAdRepository : ILostPetAdRepository
{
    private readonly ApplicationDbContext _context;

    public LostPetAdRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Kayıp ilanını eklemek için metod
    public async Task CreateLostPetAdAsync(LostPetAd lostPetAd)
    {
        await _context.LostPetAds.AddAsync(lostPetAd);
        await _context.SaveChangesAsync();
    }

    // Kayıp ilanlarının tamamını almak için metod
    public async Task<IEnumerable<LostPetAd>> GetAllAsync()
    {
        return await _context.LostPetAds.ToListAsync();
    }

    // Kayıp ilanını ID'ye göre almak için metod
    public async Task<LostPetAd> GetByIdAsync(int id)
    {
        return await _context.LostPetAds
            .FirstOrDefaultAsync(ad => ad.Id == id);
    }

    // Kayıp ilanını güncellemek için metod
    public async Task UpdateLostPetAdAsync(LostPetAd lostPetAd)
    {
        _context.LostPetAds.Update(lostPetAd);
        await _context.SaveChangesAsync();
    }

    // Kayıp ilanını silmek için metod
    public async Task DeleteLostPetAdAsync(LostPetAd lostPetAd)
    {
        _context.LostPetAds.Remove(lostPetAd);
        await _context.SaveChangesAsync();
    }
}