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
            // Simulated hashing for demonstration
            var user = await _userRepository.GetAllAsync();
            return user.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);
        }

        public async Task RegisterAsync(User user)
        {
            // Hash password in real application
            await _userRepository.AddAsync(user);
        }
    }
}
