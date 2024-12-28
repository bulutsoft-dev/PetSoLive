using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;

public class HelpRequest
{
    public int Id { get; set; }
    public string Description { get; set; }
    public EmergencyLevel EmergencyLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}