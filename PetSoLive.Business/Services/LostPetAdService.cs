using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LostPetAdService : ILostPetAdService
{
    private readonly ILostPetAdRepository _lostPetAdRepository;

    public LostPetAdService(ILostPetAdRepository lostPetAdRepository)
    {
        _lostPetAdRepository = lostPetAdRepository;
    }

    // Kayıp ilanı oluşturma
    public async Task CreateLostPetAdAsync(LostPetAd lostPetAd)
    {
        await _lostPetAdRepository.CreateLostPetAdAsync(lostPetAd);
    }

    // Şehir/bölgeye göre kayıp ilanlarını alma
    public async Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location)
    {
        return await _lostPetAdRepository.GetLostPetAdsByLocationAsync(location);
    }

    // ID'ye göre kayıp ilanını alma
    public async Task<LostPetAd> GetLostPetAdByIdAsync(int id)
    {
        return await _lostPetAdRepository.GetLostPetAdByIdAsync(id);
    }

    // Coğrafi bildirimler için yakın ilanları arama (Eğer coğrafi bildirimler eklenirse)
    public async Task<List<LostPetAd>> GetLostPetAdsNearbyAsync(string location, double radius)
    {
        // Geçici olarak coğrafi hesaplamalar yapılabilir
        // Gerçek uygulamada, harita ve coğrafi koordinatlar kullanılarak daha doğru sonuçlar alınabilir
        return await _lostPetAdRepository.GetLostPetAdsNearbyAsync(location, radius);
    }
}