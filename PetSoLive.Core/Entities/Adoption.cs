namespace PetSoLive.Core.Entities;

public class Adoption
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int UserId { get; set; }
    public DateTime AdoptionDate { get; set; }
}