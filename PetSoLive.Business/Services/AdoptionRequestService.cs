using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Business.Services
{
    public class AdoptionRequestService : IAdoptionRequestService
    {
        private readonly IAdoptionRepository _adoptionRepository;

        public AdoptionRequestService(IAdoptionRepository adoptionRepository)
        {
            _adoptionRepository = adoptionRepository;
        }

        public async Task<AdoptionRequest> GetAdoptionRequestByIdAsync(int requestId)
        {
            // Implementasyon, örneğin bir repository üzerinden veri çekebilir
            // Bu sadece bir örnektir, gerçek implementasyon veri tabanına bağlıdır
            throw new NotImplementedException("GetAdoptionRequestByIdAsync not implemented.");
        }

        public async Task UpdateAdoptionRequestAsync(AdoptionRequest request)
        {
            // Implementasyon, örneğin bir repository üzerinden veri güncelleyebilir
            throw new NotImplementedException("UpdateAdoptionRequestAsync not implemented.");
        }

        public async Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId)
        {
            return await _adoptionRepository.GetPendingRequestsByPetIdAsync(petId);
        }
    }
}