using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IPetOwnerRepository _petOwnerRepository; // PetOwnerRepository for managing ownership relations

    public PetService(IPetRepository petRepository, IPetOwnerRepository petOwnerRepository)
    {
        _petRepository = petRepository;
        _petOwnerRepository = petOwnerRepository;
    }

    // Create a new pet
    public async Task CreatePetAsync(Pet? pet)
    {
        if (pet == null) throw new ArgumentNullException(nameof(pet));
        await _petRepository.AddAsync(pet);
    }

    // Get all pets
    public async Task<IEnumerable<Pet>> GetAllPetsAsync()
    {
        return await _petRepository.GetAllAsync();
    }

    // Get a pet by its ID
    public async Task<Pet> GetPetByIdAsync(int id)
    {
        var pet = await _petRepository.GetByIdAsync(id);
        if (pet == null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }
        return pet;
    }

    // Check if the user is the owner of the pet
    public async Task<bool> IsUserOwnerOfPetAsync(int petId, int userId)
    {
        var petOwners = await _petRepository.GetPetOwnersAsync(petId);
        return petOwners.Any(po => po.UserId == userId); // Check if the user owns this pet
    }

    // Update an existing pet
    public async Task UpdatePetAsync(int petId, Pet updatedPet, int userId)
    {
        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }

        // Check if the user is the owner of the pet
        if (!await IsUserOwnerOfPetAsync(petId, userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to update this pet.");
        }

        // Update pet properties
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

        // Save the updated pet
        await _petRepository.UpdateAsync(pet);
    }

    // Assign pet ownership
    public async Task AssignPetOwnerAsync(PetOwner petOwner)
    {
        if (petOwner == null) throw new ArgumentNullException(nameof(petOwner));
        await _petOwnerRepository.AddAsync(petOwner);
        await _petOwnerRepository.SaveChangesAsync();  // Commit changes to DB
    }

    // Delete a pet
    public async Task DeletePetAsync(int petId, int userId)
    {
        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }

        // Check if the user is the owner of the pet
        if (!await IsUserOwnerOfPetAsync(petId, userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this pet.");
        }

        // Delete the pet
        await _petRepository.DeleteAsync(pet);
    }
}
