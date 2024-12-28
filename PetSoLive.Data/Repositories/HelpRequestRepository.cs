using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Data.Repositories;

public class HelpRequestRepository : IHelpRequestRepository
{
    private readonly ApplicationDbContext _context;

    public HelpRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateHelpRequestAsync(HelpRequest helpRequest)
    {
        await _context.HelpRequests.AddAsync(helpRequest);
        await _context.SaveChangesAsync();
    }

    public async Task<List<HelpRequest>> GetHelpRequestsByUserAsync(int userId)
    {
        return await _context.HelpRequests
            .Where(hr => hr.UserId == userId)
            .ToListAsync();
    }
}
