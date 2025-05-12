using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Business.Services
{
    public class AdoptionRequestService : IAdoptionRequestService
    {
        private readonly IAdoptionRequestRepository _adoptionRequestRepository;

        public AdoptionRequestService(IAdoptionRequestRepository adoptionRequestRepository)
        {
            _adoptionRequestRepository = adoptionRequestRepository ?? throw new ArgumentNullException(nameof(adoptionRequestRepository));
        }

        public async Task<AdoptionRequest> GetAdoptionRequestByIdAsync(int requestId)
        {
            var request = await _adoptionRequestRepository.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new KeyNotFoundException($"Adoption request with ID {requestId} not found.");
            }
            return request;
        }

        public async Task UpdateAdoptionRequestAsync(AdoptionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            await _adoptionRequestRepository.UpdateAsync(request);
        }

        public async Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId)
        {
            return await _adoptionRequestRepository.GetPendingRequestsByPetIdAsync(petId);
        }

        public async Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId)
        {
            return await _adoptionRequestRepository.GetAdoptionRequestsByPetIdAsync(petId);
        }
    }
}