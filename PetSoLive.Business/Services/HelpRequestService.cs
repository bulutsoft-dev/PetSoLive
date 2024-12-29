using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetSoLive.Core.Enums;

namespace PetSoLive.Business.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IHelpRequestRepository _helpRequestRepository;

        public HelpRequestService(IHelpRequestRepository helpRequestRepository)
        {
            _helpRequestRepository = helpRequestRepository;
        }

        // Create a new help request
        public async Task CreateHelpRequestAsync(HelpRequest helpRequest)
        {
            helpRequest.Status = HelpRequestStatus.Active; // Status otomatik olarak Active yapılıyor
            await _helpRequestRepository.CreateHelpRequestAsync(helpRequest);
        }

        // Get all help requests
        public async Task<List<HelpRequest>> GetHelpRequestsAsync()
        {
            return await _helpRequestRepository.GetHelpRequestsAsync();
        }

        // Get a specific help request by ID
        public async Task<HelpRequest> GetHelpRequestByIdAsync(int id)
        {
            return await _helpRequestRepository.GetHelpRequestByIdAsync(id);
        }

        // Get help requests for a specific user
        public async Task<List<HelpRequest>> GetHelpRequestsByUserAsync(int userId)
        {
            return await _helpRequestRepository.GetHelpRequestsByUserAsync(userId);
        }

        // Update an existing help request
        public async Task UpdateHelpRequestAsync(HelpRequest helpRequest)
        {
            await _helpRequestRepository.UpdateHelpRequestAsync(helpRequest);
        }

        // Delete a help request by ID
        public async Task DeleteHelpRequestAsync(int id)
        {
            await _helpRequestRepository.DeleteHelpRequestAsync(id);
        }
    }
}