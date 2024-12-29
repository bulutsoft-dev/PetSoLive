using PetSoLive.Core.Entities;

public interface ILostPetAdService
{
    Task CreateLostPetAdAsync(LostPetAd lostPetAd);
    Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location);
    Task<LostPetAd> GetLostPetAdByIdAsync(int id);  // Detayları almak için yeni metod
    Task<List<LostPetAd>> GetLostPetAdsNearbyAsync(string location, double radius);  // Coğrafi bildirimler için yeni metod
}