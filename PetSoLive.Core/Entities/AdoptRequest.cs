using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;

public class AdoptionRequest
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Message { get; set; }
    public AdoptionStatus Status { get; set; }  // Add Status property
    public DateTime RequestDate { get; set; }

    // Navigation Properties
    public Pet Pet { get; set; }
    public User User { get; set; }
}