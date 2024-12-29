using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface IHelpRequestService
    {
        // Create a new help request
        Task CreateHelpRequestAsync(HelpRequest helpRequest);

        // Get all help requests
        Task<List<HelpRequest>> GetHelpRequestsAsync();

        // Get a specific help request by ID
        Task<HelpRequest> GetHelpRequestByIdAsync(int id);

        Task UpdateHelpRequestAsync(HelpRequest helpRequest); // Yeni metot
        Task DeleteHelpRequestAsync(int id);
    }
}