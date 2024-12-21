using System;
using System.Threading.Tasks;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Business.Services
{
    public class AdoptionService : IAdoptionService
    {
        private readonly IAdoptionRepository _adoptionRepository;

        public AdoptionService(IAdoptionRepository adoptionRepository)
        {
            _adoptionRepository = adoptionRepository ?? throw new ArgumentNullException(nameof(adoptionRepository));
        }

        public async Task CreateAdoptionAsync(Adoption adoption)
        {
            if (adoption == null)
                throw new ArgumentNullException(nameof(adoption));

            // Petin zaten evlat edinilmiş olup olmadığını kontrol edin
            var isAlreadyAdopted = await _adoptionRepository.IsPetAlreadyAdoptedAsync(adoption.PetId);
            if (isAlreadyAdopted)
                throw new InvalidOperationException("This pet has already been adopted.");

            await _adoptionRepository.AddAsync(adoption);
        }

        
        public async Task<Adoption?> GetAdoptionByPetIdAsync(int petId)
        {
            // Pet'in sahiplenilip sahiplenilmediğini kontrol et
            return await _adoptionRepository.GetAdoptionByPetIdAsync(petId);
        }
        
        public async Task CreateAdoptionRequestAsync(AdoptionRequest adoptionRequest)
        {
            if (adoptionRequest == null)
                throw new ArgumentNullException(nameof(adoptionRequest));

            // Check if pet is already adopted
            var existingAdoption = await _adoptionRepository.GetAdoptionByPetIdAsync(adoptionRequest.PetId);
            if (existingAdoption != null)
                throw new InvalidOperationException("This pet has already been adopted.");

            // Save adoption request
            await _adoptionRepository.AddAsync(adoptionRequest);
        }


        // Get adoption request by pet ID
        public async Task<AdoptionRequest?> GetAdoptionRequestByPetIdAsync(int petId)
        {
            return await _adoptionRepository.GetAdoptionRequestByPetIdAsync(petId);
        }

        // Mark adoption as approved or rejected
        public async Task UpdateAdoptionStatusAsync(int adoptionRequestId, AdoptionStatus status)
        {
            var adoptionRequest = await _adoptionRepository.GetAdoptionRequestByIdAsync(adoptionRequestId);
            if (adoptionRequest == null)
            {
                throw new InvalidOperationException("Adoption request not found.");
            }

            adoptionRequest.Status = status;  // Update status of the adoption request
            await _adoptionRepository.UpdateAsync(adoptionRequest);  // Save changes
        }


    }
}