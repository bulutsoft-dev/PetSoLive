using PetSoLive.Core.Entities;
using PetSoLive.Core.DTOs;

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
        Task<IEnumerable<Pet>> GetFilteredPetsAsync(PetFilterDto filter);
        Task<IEnumerable<Pet>> GetPetsPagedAsync(int page, int pageSize);
        Task<(List<PetListItemDto> Pets, int TotalCount)> GetPetsAdvancedAsync(int page, int pageSize, string species, string color, string breed, string adoptedStatus, string search, int? ownerId);
    }
}