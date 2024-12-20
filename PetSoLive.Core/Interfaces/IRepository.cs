// /PetSoLive.Core/Interfaces/IRepository.cs

namespace PetSoLive.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task AddAsync(T entity);

    }
}