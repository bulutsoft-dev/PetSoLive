namespace PetSoLive.Core.Interfaces;

public interface IHelpRequestService
{
    Task CreateHelpRequestAsync(HelpRequest helpRequest);
    Task<List<HelpRequest>> GetHelpRequestsByUserAsync(int userId);
}
