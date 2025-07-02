using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetSoLive.Data.Repositories
{
    public class UserRepository : IUserRepository, IRepository<User>
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new user
        public async Task AddAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Get all users
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // Get a user by their ID
        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // Update an existing user
        public async Task UpdateAsync(User entity)
        {
            // DateTime alanlarını kontrol et
            if (entity.DateOfBirth != default && entity.DateOfBirth.Kind == DateTimeKind.Unspecified)
                entity.DateOfBirth = DateTime.SpecifyKind(entity.DateOfBirth, DateTimeKind.Utc);
            else if (entity.DateOfBirth != default)
                entity.DateOfBirth = entity.DateOfBirth.ToUniversalTime();

            if (entity.CreatedDate != default && entity.CreatedDate.Kind == DateTimeKind.Unspecified)
                entity.CreatedDate = DateTime.SpecifyKind(entity.CreatedDate, DateTimeKind.Utc);
            else if (entity.CreatedDate != default)
                entity.CreatedDate = entity.CreatedDate.ToUniversalTime();

            if (entity.LastLoginDate.HasValue)
            {
                if (entity.LastLoginDate.Value.Kind == DateTimeKind.Unspecified)
                    entity.LastLoginDate = DateTime.SpecifyKind(entity.LastLoginDate.Value, DateTimeKind.Utc);
                else
                    entity.LastLoginDate = entity.LastLoginDate.Value.ToUniversalTime();
            }

            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Get users based on city and district
        public async Task<List<User>> GetUsersByLocationAsync(string city, string district)
        {
            return await _context.Users
                .Where(u => u.City == city && u.District == district)
                .ToListAsync();
        }
        
    }
}