using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionRepository
    {
        Task AddAsync(Adoption adoption);
        Task AddAsync(AdoptionRequest adoptionRequest);
        Task<Adoption?> GetAdoptionByPetIdAsync(int petId);
        Task<bool> IsPetAlreadyAdoptedAsync(int petId);
        Task<AdoptionRequest?> GetAdoptionRequestByUserAndPetAsync(int userId, int petId);
        Task<List<AdoptionRequest>> GetPendingRequestsByPetIdAsync(int petId);
    }
}