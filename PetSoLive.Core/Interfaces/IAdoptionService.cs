// /PetSoLive.Core/Interfaces/IAdoptionService.cs
using PetSoLive.Core.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PetSoLive.Core.Interfaces
{
    public interface IAdoptionService
    {
        Task<IEnumerable<Adoption>> GetAllAdoptionsAsync();
        Task<Adoption> GetAdoptionByIdAsync(int id);
        Task UpdateAdoptionAsync(Adoption adoption); // Add this method to the interface
    }
}