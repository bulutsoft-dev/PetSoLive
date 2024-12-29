namespace PetSoLive.Core.Interfaces;

public interface ILostPetAdRepository
{
    Task CreateLostPetAdAsync(LostPetAd lostPetAd);
    Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location);
    Task<LostPetAd> GetLostPetAdByIdAsync(int id);
    Task<List<LostPetAd>> GetLostPetAdsNearbyAsync(string location, double radius);
}
