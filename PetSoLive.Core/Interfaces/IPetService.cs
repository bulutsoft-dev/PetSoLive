using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetService
    {
        Task CreatePetAsync(Pet? pet); // Method to create a pet
        Task<Pet?> GetByIdAsync(int petId); // Method to get a pet by ID
        
        Task<Pet> GetPetByIdAsync(int petId);  // Add this method to the interface
        Task<IEnumerable<Pet>> GetAllPetsAsync(); // This method fetches all pets

    }
}