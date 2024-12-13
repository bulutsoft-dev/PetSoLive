using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetRepository
    {
        Task AddAsync(Pet pet); // Method to add a pet
        Task<List<Pet>> GetAllAsync(); // Method to get all pets
    }
}