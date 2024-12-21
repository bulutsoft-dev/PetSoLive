using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces;

public interface IPetOwnerService
{
    Task<PetOwner> GetPetOwnerAsync(int adoptionRequestPetId);
    Task<PetOwner> GetPetOwnerByPetIdAsync(int petId);
}