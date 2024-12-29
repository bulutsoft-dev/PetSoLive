using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Entities
{
    public class Veterinarian
    {
        public int Id { get; set; }

        public int UserId { get; set; }  // Relation with User entity
        public User User { get; set; }  // Navigation property

        public string Qualifications { get; set; }
        public string ClinicAddress { get; set; }
        public string ClinicPhoneNumber { get; set; }
        public DateTime AppliedDate { get; set; }

        public VeterinarianStatus Status { get; set; }  // Enum field for status

        // Veterinarian can have comments, so this is included
        public ICollection<Comment> Comments { get; set; } // Navigation property for comments
    }
}