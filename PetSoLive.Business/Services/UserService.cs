using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        var user = await _userRepository.GetAllAsync()
                                        .ContinueWith(task => task.Result.FirstOrDefault(u => u.Username == username));

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
        var existingUser = await _userRepository.GetAllAsync()
                                                 .ContinueWith(task => task.Result.FirstOrDefault(u => u.Username == user.Username || u.Email == user.Email));

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

    // Update user details (optimized to only update changed fields)
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

        bool isUpdated = false;

        // Update only the fields that have changed
        if (!string.IsNullOrEmpty(user.Username) && user.Username != existingUser.Username)
        {
            existingUser.Username = user.Username;
            isUpdated = true;
        }

        if (!string.IsNullOrEmpty(user.Email) && user.Email != existingUser.Email)
        {
            existingUser.Email = user.Email;
            isUpdated = true;
        }

        if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber != existingUser.PhoneNumber)
        {
            existingUser.PhoneNumber = user.PhoneNumber;
            isUpdated = true;
        }

        if (!string.IsNullOrEmpty(user.Address) && user.Address != existingUser.Address)
        {
            existingUser.Address = user.Address;
            isUpdated = true;
        }

        if (user.DateOfBirth != default && user.DateOfBirth != existingUser.DateOfBirth)
        {
            existingUser.DateOfBirth = user.DateOfBirth;
            isUpdated = true;
        }

        // Hash password only if it's provided (and changed)
        if (!string.IsNullOrWhiteSpace(user.PasswordHash) && user.PasswordHash != existingUser.PasswordHash)
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            isUpdated = true;
        }

        // Update the last login date if it's provided
        if (user.LastLoginDate.HasValue && user.LastLoginDate != existingUser.LastLoginDate)
        {
            existingUser.LastLoginDate = user.LastLoginDate.Value;
            isUpdated = true;
        }

        // Update the profile image URL if it's provided
        if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != existingUser.ProfileImageUrl)
        {
            existingUser.ProfileImageUrl = user.ProfileImageUrl;
            isUpdated = true;
        }

        // If any updates were made, save to the repository
        if (isUpdated)
        {
            await _userRepository.UpdateAsync(existingUser);
        }
    }
}
