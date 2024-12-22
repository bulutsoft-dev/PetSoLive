using PetSoLive.Core.Entities;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetOwnerRepository
    {
        Task AddAsync(PetOwner petOwner);
        Task SaveChangesAsync();
        Task<PetOwner> GetPetOwnerByPetIdAsync(int petId);
    }
}