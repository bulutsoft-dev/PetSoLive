using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;

namespace PetSoLive.Data.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetCommentsByHelpRequestIdAsync(int helpRequestId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Veterinarian)
                .Where(c => c.HelpRequestId == helpRequestId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}