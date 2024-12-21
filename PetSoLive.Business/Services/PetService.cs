using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IPetOwnerRepository _petOwnerRepository; // Add dependency for PetOwnerRepository

    // Constructor for PetService
    public PetService(IPetRepository petRepository, IPetOwnerRepository petOwnerRepository)
    {
        _petRepository = petRepository;
        _petOwnerRepository = petOwnerRepository; // Inject PetOwnerRepository
    }

    // Create a new pet
    public async Task CreatePetAsync(Pet? pet)
    {
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
        return await _petRepository.GetByIdAsync(id);
    }

    // Method to check if the user is the owner of the pet
    public async Task<bool> IsUserOwnerOfPetAsync(int petId, int userId)
    {
        var petOwner = await _petRepository.GetPetOwnersAsync(petId);
        return petOwner.Any(po => po.UserId == userId); // Check if the user owns this pet
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

        // Update the pet's properties
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

    // Method to assign the pet to a user
    public async Task AssignPetOwnerAsync(PetOwner petOwner)
    {
        await _petOwnerRepository.AddAsync(petOwner); // Add PetOwner to repository
        await _petOwnerRepository.SaveChangesAsync();  // Commit the changes to the database
    }
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
