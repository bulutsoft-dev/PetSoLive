// 4. Business Logic Layer (/PetSoLive.Business)
using BCrypt.Net;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

// Add password hashing to UserService
namespace PetSoLive.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository) { _userRepository = userRepository; }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task RegisterAsync(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Şifreyi hashle
            await _userRepository.AddAsync(user);
        }

    }
}