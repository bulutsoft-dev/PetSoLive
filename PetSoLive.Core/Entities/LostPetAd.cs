using PetSoLive.Core.Entities;

public class LostPetAd
{
    public int Id { get; set; }
    public string PetName { get; set; }
    public string LastSeenLocation { get; set; } // Örneğin "İstanbul, Beşiktaş"
    public string Description { get; set; }
    public DateTime LastSeenDate { get; set; }
    public string ImageUrl { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}
