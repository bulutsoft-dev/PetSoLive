using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System;
using System.Threading.Tasks;
using PetSoLive.Core.Enums;

namespace PetSoLive.Business.Services
{
    public class AdoptionRequestService : IAdoptionRequestService
    {
        private readonly IAdoptionRequestRepository _adoptionRequestRepository;

        public AdoptionRequestService(IAdoptionRequestRepository adoptionRequestRepository)
        {
            _adoptionRequestRepository = adoptionRequestRepository ?? throw new ArgumentNullException(nameof(adoptionRequestRepository));
        }

        // Create a new adoption request
        public async Task CreateAdoptionRequestAsync(AdoptionRequest adoptionRequest)
        {
            if (adoptionRequest == null)
                throw new ArgumentNullException(nameof(adoptionRequest));

            // Save adoption request to database
            await _adoptionRequestRepository.AddAsync(adoptionRequest);
        }

        // Get adoption request by pet ID
        public async Task<AdoptionRequest?> GetAdoptionRequestByPetIdAsync(int petId)
        {
            return await _adoptionRequestRepository.GetAdoptionRequestByPetIdAsync(petId);
        }

        // Update adoption request status
        public async Task UpdateAdoptionRequestStatusAsync(int adoptionRequestId, AdoptionStatus status)
        {
            var adoptionRequest = await _adoptionRequestRepository.GetByIdAsync(adoptionRequestId);
            if (adoptionRequest == null)
            {
                throw new InvalidOperationException("Adoption request not found.");
            }

            adoptionRequest.Status = status;
            await _adoptionRequestRepository.UpdateAsync(adoptionRequest);
        }
    }


}