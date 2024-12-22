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

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);  // Update last login date
            return user;  // Successful authentication
        }

        return null;  // Invalid credentials
    }

    // Register a new user with hashed password and full details
    public async Task RegisterAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new ArgumentException("Password cannot be empty.");
        }

        // Check if username or email already exists
        var existingUser = (await _userRepository.GetAllAsync())
                           .FirstOrDefault(u => u.Username == user.Username || u.Email == user.Email);

        if (existingUser != null)
        {
            throw new ArgumentException("Username or email already exists.");
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

        return await _userRepository.GetAllAsync()
            .ContinueWith(task => task.Result.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
    }

    // Get user by ID
    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }
    
    public async Task UpdateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }

        // Ensure that the user exists in the database before updating
        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // You can also add additional validation or logic to check for specific fields that must be updated

        // Update the user properties
        existingUser.Username = user.Username ?? existingUser.Username;
        existingUser.Email = user.Email ?? existingUser.Email;
        existingUser.PhoneNumber = user.PhoneNumber ?? existingUser.PhoneNumber;
        existingUser.Address = user.Address ?? existingUser.Address;
        existingUser.DateOfBirth = user.DateOfBirth != default ? user.DateOfBirth : existingUser.DateOfBirth;

        // If the password is being updated, hash it
        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        }

        // Update the last login date if it's set
        if (user.LastLoginDate.HasValue)
        {
            existingUser.LastLoginDate = user.LastLoginDate.Value;
        }

        // Optionally, update the profile image URL
        existingUser.ProfileImageUrl = user.ProfileImageUrl ?? existingUser.ProfileImageUrl;

        // Update the user in the repository
        await _userRepository.UpdateAsync(existingUser);
    }

    
    
}
