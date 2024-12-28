using PetSoLive.Core.Interfaces;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<bool> IsUserAdminAsync(int userId)
    {
        return await _adminRepository.IsUserAdminAsync(userId);
    }
}