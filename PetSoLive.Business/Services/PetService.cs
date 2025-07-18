using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;
using PetSoLive.Core.DTOs;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IEnumerable<Pet>> GetFilteredPetsAsync(PetFilterDto filter)
    {
        var pets = await _petRepository.GetAllAsync();
        var query = pets.AsQueryable();
        if (!string.IsNullOrEmpty(filter.Species))
            query = query.Where(p => p.Species == filter.Species);
        if (!string.IsNullOrEmpty(filter.Breed))
            query = query.Where(p => p.Breed == filter.Breed);
        if (!string.IsNullOrEmpty(filter.Gender))
            query = query.Where(p => p.Gender == filter.Gender);
        if (filter.MinAge.HasValue)
            query = query.Where(p => p.Age >= filter.MinAge);
        if (filter.MaxAge.HasValue)
            query = query.Where(p => p.Age <= filter.MaxAge);
        if (!string.IsNullOrEmpty(filter.Color))
            query = query.Where(p => p.Color == filter.Color);
        if (filter.IsAdopted.HasValue)
        {
            // Adopted kontrolü: Pet'in Adoption tablosunda kaydı var mı ve status Approved mı?
            if (filter.IsAdopted.Value)
                query = query.Where(p => p.AdoptionRequests.Any(a => a.Status == PetSoLive.Core.Enums.AdoptionStatus.Approved));
            else
                query = query.Where(p => !p.AdoptionRequests.Any(a => a.Status == PetSoLive.Core.Enums.AdoptionStatus.Approved));
        }
        return query.ToList();
    }

    public async Task<(List<PetListItemDto> Pets, int TotalCount)> GetPetsAdvancedAsync(
        int page, int pageSize, string species, string color, string breed, string adoptedStatus, string search, int? ownerId)
    {
        var query = _petRepository.Query();

        if (!string.IsNullOrEmpty(species))
            query = query.Where(p => p.Species == species);
        if (!string.IsNullOrEmpty(color))
            query = query.Where(p => p.Color == color);
        if (!string.IsNullOrEmpty(breed))
            query = query.Where(p => p.Breed == breed);
        if (ownerId.HasValue)
            query = query.Where(p => p.PetOwners.Any(po => po.UserId == ownerId.Value));
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search));
        if (!string.IsNullOrEmpty(adoptedStatus))
        {
            if (adoptedStatus == "adopted")
                query = query.Where(p => p.AdoptionRequests.Any(a => a.Status == PetSoLive.Core.Enums.AdoptionStatus.Approved));
            else if (adoptedStatus == "waiting")
                query = query.Where(p => !p.AdoptionRequests.Any(a => a.Status == PetSoLive.Core.Enums.AdoptionStatus.Approved));
        }
        var totalCount = await query.CountAsync();
        var pets = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Color = p.Color,
                Breed = p.Breed,
                Age = p.Age,
                Gender = p.Gender,
                ImageUrl = p.ImageUrl,
                Description = p.Description,
                VaccinationStatus = p.VaccinationStatus,
                IsAdopted = p.AdoptionRequests.Any(a => a.Status == PetSoLive.Core.Enums.AdoptionStatus.Approved),
                AdoptedOwnerName = p.PetOwners
                    .OrderByDescending(po => po.OwnershipDate)
                    .Select(po => po.User.Username)
                    .FirstOrDefault(),
                OwnerId = p.PetOwners
                    .OrderByDescending(po => po.OwnershipDate)
                    .Select(po => po.UserId)
                    .FirstOrDefault(),
                CreatedAt = p.DateOfBirth, // Eğer ilan tarihi farklı bir property ise onu kullan
                UpdatedAt = null // Eğer güncelleme tarihi varsa onu kullan
            })
            .ToListAsync();
        return (pets, totalCount);
    }

    public async Task<IEnumerable<Pet>> GetPetsPagedAsync(int page, int pageSize)
    {
        return await _petRepository.GetPagedAsync(page, pageSize);
    }
}
