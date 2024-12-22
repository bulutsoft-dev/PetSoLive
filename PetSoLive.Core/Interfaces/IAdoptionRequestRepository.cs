using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionRequestRepository
    {
        Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId);
        Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId);
    }
}