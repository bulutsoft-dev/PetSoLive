using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetSoLive.Core.DTOs;

public class LostPetAdService : ILostPetAdService
{
    private readonly ILostPetAdRepository _lostPetAdRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly EmailHelper _emailHelper;

    public LostPetAdService(ILostPetAdRepository lostPetAdRepository, IUserRepository userRepository, IEmailService emailService)
    {
        _lostPetAdRepository = lostPetAdRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _emailHelper = new EmailHelper(); // Initialize EmailHelper
    }

    // Yeni kayıp ilanı oluşturmak için metod
    public async Task CreateLostPetAdAsync(LostPetAd lostPetAd, string city, string district)
    {
        lostPetAd.LastSeenCity = city;
        lostPetAd.LastSeenDistrict = district;

        await _lostPetAdRepository.CreateLostPetAdAsync(lostPetAd);

        var user = await _userRepository.GetByIdAsync(lostPetAd.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var usersInLocation = await _userRepository.GetUsersByLocationAsync(city, district);

        foreach (var targetUser in usersInLocation)
        {
            var subject = "New Lost Pet Ad Created";
            var body = _emailHelper.GenerateNewLostPetAdEmailBody(lostPetAd, user);
            await _emailService.SendEmailAsync(targetUser.Email, subject, body);
        }
    }

    // Kayıp ilanlarını almak için metod
    public async Task<IEnumerable<LostPetAd>> GetAllLostPetAdsAsync()
    {
        return await _lostPetAdRepository.GetAllAsync();
    }

    // Kayıp ilanını ID'ye göre almak için metod
    public async Task<LostPetAd> GetLostPetAdByIdAsync(int id)
    {
        var lostPetAd = await _lostPetAdRepository.GetByIdAsync(id);
        if (lostPetAd != null)
        {
            lostPetAd.User = await _userRepository.GetByIdAsync(lostPetAd.UserId);
        }
        return lostPetAd;
    }

    // Kayıp ilanını güncellemek için metod
    public async Task UpdateLostPetAdAsync(LostPetAd lostPetAd)
    {
        await _lostPetAdRepository.UpdateLostPetAdAsync(lostPetAd);
    }

    // Kayıp ilanını silmek için metod
    public async Task DeleteLostPetAdAsync(LostPetAd lostPetAd)
    {
        await _lostPetAdRepository.DeleteLostPetAdAsync(lostPetAd);
    }

    public async Task<IEnumerable<LostPetAd>> GetFilteredLostPetAdsAsync(LostPetAdFilterDto filterDto)
    {
        return await _lostPetAdRepository.GetFilteredAsync(filterDto);
    }
}

