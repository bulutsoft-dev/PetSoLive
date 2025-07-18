using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetRepository
    {
        Task AddAsync(Pet? pet);
        Task<List<Pet?>> GetAllAsync();
        Task<Pet?> GetByIdAsync(int petId);
        Task UpdateAsync(Pet existingPet);
        Task<List<PetOwner>> GetPetOwnersAsync(int petId);
        Task DeleteAsync(Pet pet);
        Task<List<Pet>> GetPagedAsync(int page, int pageSize);
        IQueryable<Pet> Query();
    }
}