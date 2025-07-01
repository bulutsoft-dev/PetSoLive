namespace Petsolive.API.DTOs;

public class AdoptionDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public DateTime AdoptionDate { get; set; }
    public string Status { get; set; } // Enum string olarak
}