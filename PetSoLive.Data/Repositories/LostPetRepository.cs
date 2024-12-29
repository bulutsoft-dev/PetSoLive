using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetSoLive.Data.Repositories
{
    public class LostPetAdRepository : ILostPetAdRepository
    {
        private readonly ApplicationDbContext _context;

        public LostPetAdRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kayıp ilanı oluşturma
        public async Task CreateLostPetAdAsync(LostPetAd lostPetAd)
        {
            await _context.LostPetAds.AddAsync(lostPetAd);
            await _context.SaveChangesAsync();
        }

        // Konuma göre kayıp ilanlarını alma
        public async Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location)
        {
            // Konumu tam olarak eşleştirmek yerine, daha geniş bir arama yapılabilir (örneğin, "city" içinde geçiyor)
            return await _context.LostPetAds
                .Where(ad => ad.LastSeenLocation.Contains(location))
                .ToListAsync();
        }

        // ID'ye göre kayıp ilanını alma
        public async Task<LostPetAd> GetLostPetAdByIdAsync(int id)
        {
            return await _context.LostPetAds
                .FirstOrDefaultAsync(ad => ad.Id == id);
        }

        // Coğrafi olarak yakın ilanları arama (radius ve location kullanarak)
        public async Task<List<LostPetAd>> GetLostPetAdsNearbyAsync(string location, double radius)
        {
            // Bu işlevsellik için daha gelişmiş coğrafi hesaplamalar gerekebilir. 
            // Gerçek coğrafi analiz için, örneğin lat/long verisi kullanmak gerekebilir.

            // Şu anlık, örneğin location içeren ilanları dönebiliriz.
            // Bu, gerçek harita ve coğrafi API'lere bağlanmak için temel bir placeholder olabilir.

            return await _context.LostPetAds
                .Where(ad => ad.LastSeenLocation.Contains(location))  // Burada location bazlı filtreleme yapılır
                .ToListAsync();
        }
    }
}
