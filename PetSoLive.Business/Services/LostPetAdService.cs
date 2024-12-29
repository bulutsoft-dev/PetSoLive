using PetSoLive.Core.Interfaces;

public class LostPetAdService : ILostPetAdService
{
    private readonly ILostPetAdRepository _lostPetAdRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public LostPetAdService(ILostPetAdRepository lostPetAdRepository, IUserRepository userRepository, IEmailService emailService)
    {
        _lostPetAdRepository = lostPetAdRepository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task CreateLostPetAdAsync(LostPetAd lostPetAd, string city, string district)
    {
        // Set the location details
        lostPetAd.LastSeenCity = city;
        lostPetAd.LastSeenDistrict = district;

        // Save the new lost pet ad in the database
        await _lostPetAdRepository.CreateLostPetAdAsync(lostPetAd);

        // Get users from the same location (city, district)
        var usersInLocation = await _userRepository.GetUsersByLocationAsync(city, district);

        // Send email to each user in the location
        foreach (var user in usersInLocation)
        {
            var subject = "New Lost Pet Ad Created";
            var body = $"A new lost pet ad has been posted. Pet name: {lostPetAd.PetName}, Location: {lostPetAd.LastSeenLocation}. Description: {lostPetAd.Description}.";
            await _emailService.SendEmailAsync(user.Email, subject, body);
        }
    }

    public async Task<IEnumerable<LostPetAd>> GetAllLostPetAdsAsync()
    {
        return await _lostPetAdRepository.GetAllAsync();
    }
}