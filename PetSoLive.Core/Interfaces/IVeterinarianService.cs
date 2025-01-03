using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IVeterinarianService
    {
        Task<Veterinarian> RegisterVeterinarianAsync(int userId, string qualifications, string clinicAddress, string clinicPhoneNumber);
        Task<Veterinarian> GetByUserIdAsync(int userId);
        Task<Veterinarian> GetByIdAsync(int id);  // Add this method
        Task ApproveVeterinarianAsync(int veterinarianId);
        Task RejectVeterinarianAsync(int veterinarianId);
        Task<IEnumerable<Veterinarian>> GetAllVeterinariansAsync();
        
        Task<Veterinarian> GetApprovedByUserIdAsync(int userId);
    }
}