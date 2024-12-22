using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;

namespace PetSoLive.Business.Services;

public class AdoptionRequestService : IAdoptionRequestService
{
    private readonly ApplicationDbContext _context;

    public AdoptionRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Pet'i ID'ye göre alır.
    public async Task<Pet> GetPetByIdAsync(int petId)
    {
        return await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == petId);
    }

    // Adoption request'ini ID'ye göre alır.
    public async Task<AdoptionRequest> GetAdoptionRequestByIdAsync(int requestId)
    {
        return await _context.AdoptionRequests
            .Include(ar => ar.User) // Adoption request'i ile ilişkilendirilmiş kullanıcıyı yükler.
            .FirstOrDefaultAsync(ar => ar.Id == requestId);
    }

    // Belirli bir pet'e ait tüm adoption request'lerini alır.
    public async Task<List<AdoptionRequest>> GetAdoptionRequestsByPetIdAsync(int petId)
    {
        return await _context.AdoptionRequests
            .Where(ar => ar.PetId == petId)
            .ToListAsync();
    }

    // Adoption request'ini günceller.
    public async Task UpdateAdoptionRequestAsync(AdoptionRequest request)
    {
        _context.AdoptionRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    // Pet'i günceller (Adoption durumu gibi).
    public async Task UpdatePetAsync(Pet pet)
    {
        _context.Pets.Update(pet);
        await _context.SaveChangesAsync();
    }
}