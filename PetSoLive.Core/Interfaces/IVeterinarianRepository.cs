using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IVeterinarianRepository
    {
        Task<Veterinarian> GetByUserIdAsync(int userId);
        Task<Veterinarian> CreateAsync(Veterinarian veterinarian);
        Task UpdateAsync(Veterinarian veterinarian);
        Task<IEnumerable<Veterinarian>> GetAllAsync();
        Task<Veterinarian> GetByIdAsync(int id);  // Add this method
    }
}