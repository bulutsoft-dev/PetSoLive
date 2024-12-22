// /PetSoLive.Core/Interfaces/IRepository.cs

using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task AddAsync(T entity);
        Task<T> GetByIdAsync(int id);
        Task UpdateAsync(T entity);

    }
}