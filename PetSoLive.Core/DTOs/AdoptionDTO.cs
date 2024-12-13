// PetSoLive.Core/DTOs/AdoptionDto.cs

using PetSoLive.Core.Enums;

public class AdoptionDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int UserId { get; set; }
    public DateTime AdoptionDate { get; set; }
    public AdoptionStatus Status { get; set; }  // Include the Enum here
}