namespace PetSoLive.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public int HelpRequestId { get; set; }
        public HelpRequest HelpRequest { get; set; } // Navigation property for HelpRequest

        public int UserId { get; set; }
        public User User { get; set; } // Navigation property for User who commented

        public int? VeterinarianId { get; set; }
        public Veterinarian Veterinarian { get; set; } // Navigation property for Veterinarian (optional)

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}