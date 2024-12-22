using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces;

public interface IPetOwnerService
{
    Task<PetOwner> GetPetOwnerByPetIdAsync(int petId);
}