using PetSoLive.Core.Entities;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IPetOwnerRepository
    {
        Task AddAsync(PetOwner petOwner);
        Task SaveChangesAsync();  // If you need to commit changes, depending on the repository pattern used
    }
}