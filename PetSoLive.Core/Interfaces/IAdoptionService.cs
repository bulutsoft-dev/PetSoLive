// /PetSoLive.Core/Interfaces/IAdoptionService.cs
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionService
    {
        Task CreateAdoptionAsync(Adoption adoption);

        Task<Adoption?> GetAdoptionByPetIdAsync(int petId);
    }
}