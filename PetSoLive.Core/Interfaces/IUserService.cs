// Interfaces (Abstraction for services and repositories)

using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{

    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task RegisterAsync(User user);
        
        
        
    }

}