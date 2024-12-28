using PetSoLive.Core.Interfaces;

public class LostPetAdService : ILostPetAdService
{
    private readonly ILostPetAdRepository _lostPetAdRepository;

    public LostPetAdService(ILostPetAdRepository lostPetAdRepository)
    {
        _lostPetAdRepository = lostPetAdRepository;
    }

    public async Task CreateLostPetAdAsync(LostPetAd lostPetAd)
    {
        await _lostPetAdRepository.CreateLostPetAdAsync(lostPetAd);
    }

    public async Task<List<LostPetAd>> GetLostPetAdsByLocationAsync(string location)
    {
        return await _lostPetAdRepository.GetLostPetAdsByLocationAsync(location);
    }
}