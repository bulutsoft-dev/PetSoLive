namespace Petsolive.API.DTOs;

public class AdoptionRequestDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } // Enum string olarak
    public DateTime RequestDate { get; set; }
}