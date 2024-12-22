using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionRequestRepository
    {
        Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId);
        
        Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId);  // Get all pending adoption requests for a pet
        Task<AdoptionRequest> GetByIdAsync(int adoptionRequestId);  // Get adoption request by ID
        Task UpdateAsync(AdoptionRequest adoptionRequest);  // Update adoption request status

    }
}