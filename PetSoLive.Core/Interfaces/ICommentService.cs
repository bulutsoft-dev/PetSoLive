using PetSoLive.Core.Entities;

namespace PetSoLive.Business.Services
{
    public interface ICommentService
    {
        // Yorum ekleme metodu
        Task AddCommentAsync(Comment comment);

        // Belirli bir HelpRequest için yorumları al
        Task<List<Comment>> GetCommentsByHelpRequestIdAsync(int helpRequestId);
    }
}