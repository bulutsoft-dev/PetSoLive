using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionRequestRepository
    {
        Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId);
        Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId);
        Task<AdoptionRequest> GetByIdAsync(int adoptionRequestId);
        Task UpdateAsync(AdoptionRequest adoptionRequest);
    }
}