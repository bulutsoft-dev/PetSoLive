namespace PetSoLive.Core.Interfaces
{
    public interface IAdminService
    {
        Task<bool> IsUserAdminAsync(int userId);
    }
}

