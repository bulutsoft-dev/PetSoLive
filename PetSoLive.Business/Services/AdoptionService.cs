using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;
using PetSoLive.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Business.Services
{
    public class AdoptionService : IAdoptionService
    {
        private readonly IRepository<Adoption> _adoptionRepository;

        public AdoptionService(IRepository<Adoption> adoptionRepository)
        {
            _adoptionRepository = adoptionRepository ?? throw new ArgumentNullException(nameof(adoptionRepository));
        }

        public async Task<List<Adoption>> GetAllAdoptionsAsync()
        {
            var adoptions = await _adoptionRepository.GetAllAsync();
            return adoptions != null ? new List<Adoption>(adoptions) : new List<Adoption>();
        }

        public async Task<Adoption> GetAdoptionByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Adoption ID must be greater than zero.", nameof(id));

            var adoption = await _adoptionRepository.GetByIdAsync(id);
            if (adoption == null)
                throw new KeyNotFoundException($"Adoption with ID {id} was not found.");

            return adoption;
        }

        public async Task UpdateAdoptionAsync(Adoption adoption)
        {
            if (adoption == null)
                throw new ArgumentNullException(nameof(adoption));

            await _adoptionRepository.UpdateAsync(adoption);
        }

        public async Task CreateAdoptionAsync(Adoption adoption)
        {
            if (adoption == null)
                throw new ArgumentNullException(nameof(adoption));

            await _adoptionRepository.AddAsync(adoption);
        }

        public async Task UpdateAdoptionStatusAsync(int id, AdoptionStatus status)
        {
            var adoption = await GetAdoptionByIdAsync(id);
            if (adoption == null)
                throw new KeyNotFoundException($"Adoption with ID {id} was not found.");

            adoption.Status = status;
            await _adoptionRepository.UpdateAsync(adoption);
        }
        
        
        
    }
}
