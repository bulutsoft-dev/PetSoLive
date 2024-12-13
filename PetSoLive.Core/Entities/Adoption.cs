// PetSoLive.Core/Entities/Adoption.cs

using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;  // Reference the Enum

public class Adoption
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime AdoptionDate { get; set; }
    public AdoptionStatus Status { get; set; }  // Use enum instead of string
}