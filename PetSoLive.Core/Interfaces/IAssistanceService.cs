// /PetSoLive.Core/Interfaces/IAssistanceService.cs
using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IAssistanceService
    {
        Task<IEnumerable<Assistance>> GetAllAssistancesAsync();
        Task CreateAssistanceAsync(Assistance assistance);
    }
}