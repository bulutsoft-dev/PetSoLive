using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Entities
{
    public class Veterinarian
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // User bilgileriyle ilişkilendirilmiş
        public string Qualifications { get; set; }
        public string ClinicAddress { get; set; }
        public string ClinicPhoneNumber { get; set; }
        public DateTime AppliedDate { get; set; }  // Başvuru tarihi
        
        public VeterinarianStatus Status { get; set; }  // Enum tipi status

        public User User { get; set; }  // Navigation property
    }
}