using System;
using System.Threading.Tasks;
using PetSoLive.Core.Entities;
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
    }
}