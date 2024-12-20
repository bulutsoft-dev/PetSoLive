using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Business.Services
{
    public class AdoptionService : IAdoptionService
    {
        private readonly IRepository<Adoption> _adoptionRepository;

        public AdoptionService(IRepository<Adoption> adoptionRepository)
        {
            _adoptionRepository = adoptionRepository ?? throw new ArgumentNullException(nameof(adoptionRepository));
        }

        
        public async Task CreateAdoptionAsync(Adoption adoption)
        {
            if (adoption == null)
                throw new ArgumentNullException(nameof(adoption));

            await _adoptionRepository.AddAsync(adoption);
        }
        
        
    }
}
