namespace Petsolive.API.DTOs;

public class PetOwnerDto
{
    public int PetId { get; set; }
    public string PetName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public DateTime OwnershipDate { get; set; }
}