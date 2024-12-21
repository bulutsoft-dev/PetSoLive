using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetService
    {
        Task CreatePetAsync(Pet? pet); // Method to create a pet
        
        Task<Pet> GetPetByIdAsync(int id); // Fetch pet by ID
        Task<IEnumerable<Pet>> GetAllPetsAsync(); // This method fetches all pets
        
    // Add UpdatePetAsync method
    // Add UpdatePetAsync method
    Task UpdatePetAsync(int petId, Pet updatedPet, int userId);


    Task<bool> IsUserOwnerOfPetAsync(int id, int userId);
    Task AssignPetOwnerAsync(PetOwner petOwner);
    
    Task DeletePetAsync(int petId, int userId);

    }
}