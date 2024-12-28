using Microsoft.EntityFrameworkCore;
using PetSoLive.Data;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsUserAdminAsync(int userId)
    {
        return await _context.Admins.AnyAsync(a => a.UserId == userId);
    }

    // Diğer admin veri işlemleri eklenebilir
}