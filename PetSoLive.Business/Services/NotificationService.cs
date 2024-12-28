using PetSoLive.Business.Services;

public class NotificationService : INotificationService
{
    public async Task SendNotificationAsync(string title, string message, string location)
    {
        // Bildirim gönderme mantığı (örneğin, Firebase veya SignalR ile)
        Console.WriteLine($"Notification: {title} - {message} at {location}");
        await Task.CompletedTask;
    }

    public async Task SendEmergencyNotificationAsync(string title, string message)
    {
        // Acil durum bildirimi mantığı
        Console.WriteLine($"Emergency Notification: {title} - {message}");
        await Task.CompletedTask;
    }
}