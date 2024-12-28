using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetSoLive.Data.Repositories
{
    public class HelpRequestRepository : IHelpRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public HelpRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create a new help request
        public async Task CreateHelpRequestAsync(HelpRequest helpRequest)
        {
            await _context.HelpRequests.AddAsync(helpRequest);
            await _context.SaveChangesAsync();
        }


        // Get all help requests
        public async Task<List<HelpRequest>> GetHelpRequestsAsync()
        {
            return await _context.HelpRequests
                .Include(hr => hr.User)  // Include related user data if necessary
                .OrderByDescending(hr => hr.CreatedAt)  // Order by creation time, descending
                .ToListAsync();
        }

        // Get a specific help request by ID
        public async Task<HelpRequest> GetHelpRequestByIdAsync(int id)
        {
            return await _context.HelpRequests
                .Include(hr => hr.User)  // Include related user data if necessary
                .FirstOrDefaultAsync(hr => hr.Id == id);
        }

        // Get help requests for a specific user
        public async Task<List<HelpRequest>> GetHelpRequestsByUserAsync(int userId)
        {
            return await _context.HelpRequests
                .Where(hr => hr.UserId == userId)
                .Include(hr => hr.User)  // Include related user data if necessary
                .OrderByDescending(hr => hr.CreatedAt)  // Order by creation time, descending
                .ToListAsync();
        }
    }
}