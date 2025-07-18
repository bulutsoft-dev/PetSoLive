 public class PetListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Species { get; set; }
    public string Color { get; set; }
    public string Breed { get; set; }
    public bool IsAdopted { get; set; }
    public string AdoptedOwnerName { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public string ImageUrl { get; set; }
    public string Description { get; set; }
    public string VaccinationStatus { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // Ekstra: Şehir, adoptionStatus, ownerAvatarUrl, isMine gibi alanlar da eklenebilir.
} 