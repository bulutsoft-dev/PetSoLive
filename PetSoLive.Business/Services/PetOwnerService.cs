// PetOwnerService.cs

using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

public class PetOwnerService : IPetOwnerService
{
    private readonly IPetOwnerRepository _petOwnerRepository;

    public PetOwnerService(IPetOwnerRepository petOwnerRepository)
    {
        _petOwnerRepository = petOwnerRepository;
    }

    public async Task<PetOwner> GetPetOwnerAsync(int petId)
    {
        var petOwner = await _petOwnerRepository.GetPetOwnerByPetIdAsync(petId);
        if (petOwner == null)
        {
            throw new Exception("Pet owner not found.");
        }

        return petOwner; // Ensure that petOwner includes the User navigation property
    }
}