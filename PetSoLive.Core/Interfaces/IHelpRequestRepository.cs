namespace PetSoLive.Core.Interfaces;

public interface IHelpRequestRepository
{
    Task CreateHelpRequestAsync(HelpRequest helpRequest);
    Task<List<HelpRequest>> GetHelpRequestsByUserAsync(int userId);
}
