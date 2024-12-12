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

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return _repositoryImplementation.GetAllAsync();
    }

    public async Task AddAsync(User entity) => await _context.Users.AddAsync(entity);

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