using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetService
    {
        Task CreatePetAsync(Pet? pet);
        Task<Pet> GetPetByIdAsync(int id);
        Task<IEnumerable<Pet>> GetAllPetsAsync();
        Task UpdatePetAsync(int petId, Pet updatedPet, int userId);
        Task<bool> IsUserOwnerOfPetAsync(int id, int userId);
        Task AssignPetOwnerAsync(PetOwner petOwner);
        Task DeletePetAsync(int petId, int userId);
        Task DeletePetOwnerAsync(int petId, int userId);
    }
}