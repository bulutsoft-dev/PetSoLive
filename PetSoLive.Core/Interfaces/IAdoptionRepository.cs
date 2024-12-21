using PetSoLive.Core.Entities;

public interface IAdoptionRepository
{
    Task AddAsync(Adoption adoption);
    Task AddAsync(AdoptionRequest adoptionRequest);
    Task<Adoption?> GetAdoptionByPetIdAsync(int petId);
    Task<AdoptionRequest?> GetAdoptionRequestByPetIdAsync(int petId);
    Task<AdoptionRequest?> GetAdoptionRequestByIdAsync(int id);
    Task UpdateAsync(AdoptionRequest adoptionRequest);
    Task UpdateAsync(Adoption adoption);
    Task<bool> IsPetAlreadyAdoptedAsync(int petId);
    
    
}