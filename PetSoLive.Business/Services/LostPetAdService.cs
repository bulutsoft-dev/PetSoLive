using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // Get the user who posted the ad
        var user = await _userRepository.GetByIdAsync(lostPetAd.UserId);

        // Ensure the user exists
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Get users from the same location (city, district)
        var usersInLocation = await _userRepository.GetUsersByLocationAsync(city, district);

        // Send email to each user in the location
        foreach (var targetUser in usersInLocation)
        {
            var subject = "New Lost Pet Ad Created";
            var body = $@"
                A new lost pet ad has been posted.
                Pet Name: {lostPetAd.PetName}
                Location: {lostPetAd.LastSeenLocation}
                Description: {lostPetAd.Description}
                Posted by: {user.Username} ({user.Email})
                Contact: {user.PhoneNumber}
            ";
            await _emailService.SendEmailAsync(targetUser.Email, subject, body);
        }
    }

    public async Task<IEnumerable<LostPetAd>> GetAllLostPetAdsAsync()
    {
        return await _lostPetAdRepository.GetAllAsync();
    }

    public async Task<LostPetAd> GetLostPetAdByIdAsync(int id)
    {
        // Fetch the lost pet ad and include the associated user (eager loading)
        var lostPetAd = await _lostPetAdRepository.GetByIdAsync(id);

        if (lostPetAd != null)
        {
            // Ensure the user information is loaded as well
            lostPetAd.User = await _userRepository.GetByIdAsync(lostPetAd.UserId);
        }

        return lostPetAd;
    }
}
