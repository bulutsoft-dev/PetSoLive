using PetSoLive.Core.Entities;

public interface IAdoptionRepository
{
    Task AddAsync(Adoption adoption);
    Task AddAsync(AdoptionRequest adoptionRequest);
    Task<Adoption?> GetAdoptionByPetIdAsync(int petId);
    Task<bool> IsPetAlreadyAdoptedAsync(int petId);
    Task<AdoptionRequest?> GetAdoptionRequestByUserAndPetAsync(int userId, int petId);
}