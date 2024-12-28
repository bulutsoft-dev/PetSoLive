namespace PetSoLive.Core.Interfaces;

public interface ILostPetAdRepository
{
    Task CreateLostPetAdAsync(LostPetAd lostPetAd);
    Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location);
}
