// /PetSoLive.Core/Interfaces/IAdoptionService.cs
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionService
    {
        Task<List<Adoption>> GetAllAdoptionsAsync();
        Task<Adoption> GetAdoptionByIdAsync(int id);
        Task UpdateAdoptionAsync(Adoption adoption);
        Task CreateAdoptionAsync(Adoption adoption);
        Task UpdateAdoptionStatusAsync(int id, AdoptionStatus status);
    }
}