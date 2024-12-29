using PetSoLive.Core.Entities;

namespace PetSoLive.Core.Interfaces
{
    public interface ICommentRepository
    {
        Task AddCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByHelpRequestIdAsync(int helpRequestId);
        Task<Comment> GetCommentByIdAsync(int id);
        Task UpdateCommentAsync(Comment comment);
        Task DeleteCommentAsync(int id);
    }
}