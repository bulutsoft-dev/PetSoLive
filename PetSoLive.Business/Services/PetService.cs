using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

namespace PetSoLive.Business.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        public async Task CreatePetAsync(Pet pet)
        {
            await _petRepository.AddAsync(pet);
        }

        public async Task<List<Pet>> GetAllPetsAsync()
        {
            return await _petRepository.GetAllAsync();
        }
    }
}