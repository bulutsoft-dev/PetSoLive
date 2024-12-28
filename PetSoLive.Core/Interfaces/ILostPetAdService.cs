public interface ILostPetAdService
{
    Task CreateLostPetAdAsync(LostPetAd lostPetAd);
    Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location);
}