using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces;

public interface IAdoptionRepository : IRepository<Adoption>
{
    Task<bool> IsPetAlreadyAdoptedAsync(int adoptionPetId);

    Task<Adoption?> GetAdoptionByPetIdAsync(int petId);
}
