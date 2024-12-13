// /PetSoLive.Business/Services/AdoptionService.cs
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PetSoLive.Business.Services
{
    public class AdoptionService : IAdoptionService
    {
        private readonly IRepository<Adoption> _adoptionRepository;

        public AdoptionService(IRepository<Adoption> adoptionRepository)
        {
            _adoptionRepository = adoptionRepository;
        }

        public async Task<IEnumerable<Adoption>> GetAllAdoptionsAsync()
        {
            return await _adoptionRepository.GetAllAsync();
        }

        public async Task<Adoption> GetAdoptionByIdAsync(int id)
        {
            return await _adoptionRepository.GetByIdAsync(id);
        }

        public async Task UpdateAdoptionAsync(Adoption adoption)
        {
            await _adoptionRepository.UpdateAsync(adoption);
        }

        public async Task CreateAdoptionAsync(Adoption adoption)
        {
            await _adoptionRepository.AddAsync(adoption);
        }
    }
}