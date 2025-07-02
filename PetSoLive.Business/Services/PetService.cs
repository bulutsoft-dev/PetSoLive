using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IPetOwnerRepository _petOwnerRepository;

    public PetService(IPetRepository petRepository, IPetOwnerRepository petOwnerRepository)
    {
        _petRepository = petRepository;
        _petOwnerRepository = petOwnerRepository;
    }

    public async Task CreatePetAsync(Pet? pet)
    {
        if (pet == null) throw new ArgumentNullException(nameof(pet));
        await _petRepository.AddAsync(pet);
    }

    public async Task<IEnumerable<Pet>> GetAllPetsAsync()
    {
        return await _petRepository.GetAllAsync();
    }

    public async Task<Pet> GetPetByIdAsync(int id)
    {
        var pet = await _petRepository.GetByIdAsync(id);
        if (pet == null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }
        return pet;
    }

    public async Task<bool> IsUserOwnerOfPetAsync(int petId, int userId)
    {
        var petOwners = await _petRepository.GetPetOwnersAsync(petId);
        return petOwners.Any(po => po.UserId == userId);
    }

    public async Task UpdatePetAsync(int petId, Pet updatedPet, int userId)
    {
        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }

        if (!await IsUserOwnerOfPetAsync(petId, userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to update this pet.");
        }

        pet.Name = updatedPet.Name;
        pet.Species = updatedPet.Species;
        pet.Breed = updatedPet.Breed;
        pet.Age = updatedPet.Age;
        pet.Gender = updatedPet.Gender;
        pet.Weight = updatedPet.Weight;
        pet.Color = updatedPet.Color;
        pet.DateOfBirth = updatedPet.DateOfBirth;
        pet.Description = updatedPet.Description;
        pet.VaccinationStatus = updatedPet.VaccinationStatus;
        pet.MicrochipId = updatedPet.MicrochipId;
        pet.IsNeutered = updatedPet.IsNeutered;
        pet.ImageUrl = updatedPet.ImageUrl;

        await _petRepository.UpdateAsync(pet);
    }

    public async Task AssignPetOwnerAsync(PetOwner petOwner)
    {
        if (petOwner == null) throw new ArgumentNullException(nameof(petOwner));
        await _petOwnerRepository.AddAsync(petOwner);
        await _petOwnerRepository.SaveChangesAsync();
    }

    public async Task DeletePetAsync(int petId, int userId)
    {
        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }

        if (!await IsUserOwnerOfPetAsync(petId, userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this pet.");
        }

        await _petRepository.DeleteAsync(pet);
    }

    public async Task DeletePetOwnerAsync(int petId, int userId)
    {
        await _petOwnerRepository.DeleteAsync(petId, userId);
        await _petOwnerRepository.SaveChangesAsync();
    }
}
