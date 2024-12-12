using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Data.Repositories;

public class UserRepository : IRepository<User>
{
    private readonly ApplicationDbContext _context;
    private IRepository<User> _repositoryImplementation;

    public UserRepository(ApplicationDbContext context) { _context = context; }
    
    
    
    
    
    public Task<User> GetByIdAsync(int id)
    {
        return _repositoryImplementation.GetByIdAsync(id);
    }

    public async Task AddAsync(User entity)
    {
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync(); // Veritabanına işlemleri yazar
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public Task UpdateAsync(User entity)
    {
        return _repositoryImplementation.UpdateAsync(entity);
    }

    public Task DeleteAsync(int id)
    {
        return _repositoryImplementation.DeleteAsync(id);
    }
    // Implement other IRepository methods
}