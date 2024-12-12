// 4. Business Logic Layer (/PetSoLive.Business)

using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository) { _userRepository = userRepository; }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Authentication logic
            return await _userRepository.GetByIdAsync(1); // Example
        }

        public async Task RegisterAsync(User user)
        {
            // Registration logic
            await _userRepository.AddAsync(user);
        }
    }

}