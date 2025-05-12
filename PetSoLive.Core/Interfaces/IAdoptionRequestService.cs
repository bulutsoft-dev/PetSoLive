using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionRequestService
    {
        Task<AdoptionRequest> GetAdoptionRequestByIdAsync(int requestId);
        Task UpdateAdoptionRequestAsync(AdoptionRequest request);
        Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId);
        Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId);
    }
}