using PetSoLive.Core.Interfaces;

public class HelpRequestService : IHelpRequestService
{
    private readonly IHelpRequestRepository _helpRequestRepository;

    public HelpRequestService(IHelpRequestRepository helpRequestRepository)
    {
        _helpRequestRepository = helpRequestRepository;
    }

    public async Task CreateHelpRequestAsync(HelpRequest helpRequest)
    {
        await _helpRequestRepository.CreateHelpRequestAsync(helpRequest);
    }

    public async Task<List<HelpRequest>> GetHelpRequestsByUserAsync(int userId)
    {
        return await _helpRequestRepository.GetHelpRequestsByUserAsync(userId);
    }
}