using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task RegisterAsync(User user);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(User user);
    }
}