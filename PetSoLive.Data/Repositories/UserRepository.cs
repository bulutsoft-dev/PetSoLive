using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetSoLive.Data.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }
        
        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);  // Example with EF Core
        }
        
        // Implement UpdateAsync method
        public async Task UpdateAsync(User entity)
        {
            _context.Users.Update(entity);  // This will mark the user entity as modified
            await _context.SaveChangesAsync();  // Save changes to the database
        }
        
    }
}