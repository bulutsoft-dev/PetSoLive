namespace Petsolive.API.DTOs;

public class LostPetAdFilterDto
{
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PetType { get; set; }
    public DateTime? DatePostedAfter { get; set; }
}