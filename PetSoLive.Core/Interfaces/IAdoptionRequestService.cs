using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces;

public interface IAdoptionRequestService
{
    Task<AdoptionRequest> GetAdoptionRequestByIdAsync(int requestId);
    Task UpdateAdoptionRequestAsync(AdoptionRequest request);
}
