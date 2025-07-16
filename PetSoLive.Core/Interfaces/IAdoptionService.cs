using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionService
    {
        Task<Adoption?> GetAdoptionByPetIdAsync(int petId);
        Task CreateAdoptionRequestAsync(AdoptionRequest adoptionRequest);
        Task<AdoptionRequest?> GetAdoptionRequestByUserAndPetAsync(int userId, int petId);
        Task CreateAdoptionAsync(Adoption adoption);
        Task<Adoption?> GetAdoptionByPetAndUserAsync(int petId, int userId);
        Task<bool> IsPetAlreadyAdoptedAsync(int petId);
        Task<IEnumerable<Adoption>> GetAdoptionsPagedAsync(int page, int pageSize);
    }
}