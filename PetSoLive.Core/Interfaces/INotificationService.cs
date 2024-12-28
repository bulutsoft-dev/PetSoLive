namespace PetSoLive.Business.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string title, string message, string location);
    Task SendEmergencyNotificationAsync(string title, string message);
}