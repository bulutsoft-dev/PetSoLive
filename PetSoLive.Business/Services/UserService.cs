using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PetSoLive.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly string _secretKey;

        public UserService(IRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _secretKey = configuration["JwtSettings:SecretKey"]; // This should work
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

            // Hash the user's password before saving to the database
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); 
            await _userRepository.AddAsync(user);
        }

        // Generate JWT token for authenticated user
        private string GenerateJwtToken(User user)
        {
            // Claims can be extended for further user-specific details (e.g., roles, permissions)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, string.Join(",", user.Roles)) // Joining roles as a comma-separated string
            };

            // Use the secret key from configuration to sign the token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create and return the JWT token
            var token = new JwtSecurityToken(
                issuer: "PetSoLive",
                audience: "PetSoLiveUser",
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Token expiration time (1 hour)
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // Return the JWT as a string
        }
    }
}
