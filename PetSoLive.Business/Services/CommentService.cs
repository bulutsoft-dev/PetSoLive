using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetSoLive.Business.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        // Add a new comment
        public async Task AddCommentAsync(Comment comment)
        {
            await _commentRepository.AddCommentAsync(comment);
        }

        // Get all comments for a specific HelpRequest
        public async Task<List<Comment>> GetCommentsByHelpRequestIdAsync(int helpRequestId)
        {
            return await _commentRepository.GetCommentsByHelpRequestIdAsync(helpRequestId);
        }

        // Get a specific comment by its ID
        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _commentRepository.GetCommentByIdAsync(id);
        }

        // Update an existing comment
        public async Task UpdateCommentAsync(Comment comment)
        {
            await _commentRepository.UpdateCommentAsync(comment);
        }

        // Delete a comment
        public async Task DeleteCommentAsync(int id)
        {
            await _commentRepository.DeleteCommentAsync(id);
        }
    }
}