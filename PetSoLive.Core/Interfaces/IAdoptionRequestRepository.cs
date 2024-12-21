using PetSoLive.Core.Entities;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionRequestRepository
    {
        Task AddAsync(AdoptionRequest adoptionRequest);
        Task<AdoptionRequest?> GetAdoptionRequestByPetIdAsync(int petId);
        Task<AdoptionRequest?> GetByIdAsync(int id);
        Task UpdateAsync(AdoptionRequest adoptionRequest);
    }
}