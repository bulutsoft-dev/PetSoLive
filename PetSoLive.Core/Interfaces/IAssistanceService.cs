// /PetSoLive.Core/Interfaces/IAssistanceService.cs
using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface IAssistanceService
    {
        Task<IEnumerable<Assistance>> GetAllAssistancesAsync();
        Task CreateAssistanceAsync(Assistance assistance);
    }
}