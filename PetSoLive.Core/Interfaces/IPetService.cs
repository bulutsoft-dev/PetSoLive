using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetService
    {
        Task CreatePetAsync(Pet pet); // Method to create a pet
        Task<List<Pet>> GetAllPetsAsync(); // Method to get all pets
    }
}