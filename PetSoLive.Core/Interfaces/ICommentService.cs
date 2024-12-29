using PetSoLive.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Core.Interfaces
{
    public interface ICommentService
    {
        // Add a new comment
        Task AddCommentAsync(Comment comment);

        // Get all comments for a specific HelpRequest
        Task<List<Comment>> GetCommentsByHelpRequestIdAsync(int helpRequestId);

        // Get a specific comment by its ID
        Task<Comment> GetCommentByIdAsync(int id);

        // Update an existing comment
        Task UpdateCommentAsync(Comment comment);

        // Delete a comment
        Task DeleteCommentAsync(int id);
    }
}