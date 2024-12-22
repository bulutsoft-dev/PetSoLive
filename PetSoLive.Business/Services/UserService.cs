using Microsoft.Extensions.Configuration;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
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

    // Register a new user with hashed password and full details
    public async Task RegisterAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new ArgumentException("Password cannot be empty.");
        }

        if (user.Roles == null || !user.Roles.Any())
        {
            user.Roles = new List<string> { "User" }; // Default role
        }

        // Hash the user's password before saving to the database
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedDate = DateTime.UtcNow;
        user.IsActive = true;

        await _userRepository.AddAsync(user);
    }

    // Get user by username
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));
        }

        // Retrieve the user by username
        return await _userRepository.GetAllAsync()
            .ContinueWith(task => task.Result.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
    }

    // Get user by ID
    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }
}
