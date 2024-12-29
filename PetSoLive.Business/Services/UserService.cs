using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    // Kullanıcıyı kimlik ve şifre ile doğrulama
    public async Task<User> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetAllAsync()
                                        .ContinueWith(task => task.Result.FirstOrDefault(u => u.Username == username));

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return user;
        }

        return null;
    }

    // Yeni bir kullanıcı kaydetme
    public async Task RegisterAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new ArgumentException("Password cannot be empty.");
        }

        var existingUser = await _userRepository.GetAllAsync()
                                                 .ContinueWith(task => task.Result.FirstOrDefault(u => u.Username == user.Username || u.Email == user.Email));

        if (existingUser != null)
        {
            throw new ArgumentException("Username or email already exists.");
        }

        if (user.Roles == null || !user.Roles.Any())
        {
            user.Roles = new List<string> { "User" };
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedDate = DateTime.UtcNow;
        user.IsActive = true;

        await _userRepository.AddAsync(user);
    }

    // Kullanıcıyı kullanıcı adı ile almak
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));
        }

        return await _userRepository.GetAllAsync()
            .ContinueWith(task => task.Result.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
    }

    // Kullanıcıyı ID ile almak
    public async Task<User> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return user;
    }

    // Kullanıcı bilgilerini güncelleme
    public async Task UpdateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }

        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        bool isUpdated = false;

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

        if (!string.IsNullOrWhiteSpace(user.PasswordHash) && user.PasswordHash != existingUser.PasswordHash)
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            isUpdated = true;
        }

        if (user.LastLoginDate.HasValue && user.LastLoginDate != existingUser.LastLoginDate)
        {
            existingUser.LastLoginDate = user.LastLoginDate.Value;
            isUpdated = true;
        }

        if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != existingUser.ProfileImageUrl)
        {
            existingUser.ProfileImageUrl = user.ProfileImageUrl;
            isUpdated = true;
        }

        if (isUpdated)
        {
            await _userRepository.UpdateAsync(existingUser);
        }
    }

    // Kullanıcıları şehir ve ilçe bazında filtreleme
    public async Task<List<User>> GetUsersByLocationAsync(string city, string district)
    {
        // _userRepository.GetAllAsync() asenkron bir şekilde dönecek, fakat sonrasında IQueryable işlemi yapılabilir
        var usersInLocation = (await _userRepository.GetAllAsync()) // Verileri asenkron olarak al
            .Where(u => u.City == city && u.District == district) // Filtreleme işlemi burada yapılır
            .ToList(); // Sonuçları listeye çevir

        return usersInLocation;
    }
}
