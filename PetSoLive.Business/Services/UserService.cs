using Microsoft.Extensions.Configuration;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;


namespace PetSoLive.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
        }


        // Authenticate user by verifying username and password
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Username == username);

            // If user exists and password is correct, return the user
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user; // Successful authentication
            }

            return null; // Invalid credentials
        }

        // Register a new user with hashed password
        public async Task RegisterAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                throw new ArgumentException("Password cannot be empty.");
            }

            // Ensure the user has a default role if no roles are provided
            if (user.Roles == null || !user.Roles.Any())
            {
                user.Roles = new List<string> { "User" }; // Default role
            }

            // Hash the user's password before saving to the database
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); 
            await _userRepository.AddAsync(user);
        }
        
    }
}
