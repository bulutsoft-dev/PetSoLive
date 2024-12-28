public interface IAdminRepository
{
    Task<bool> IsUserAdminAsync(int userId);
    // Diğer admin işlemleri eklenebilir
}