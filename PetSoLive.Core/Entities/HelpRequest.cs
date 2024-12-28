using PetSoLive.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace PetSoLive.Core.Entities
{
    public class HelpRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Emergency Level is required.")]
        public EmergencyLevel EmergencyLevel { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public int UserId { get; set; }  // Foreign Key

        public User? User { get; set; }    // Make this property nullable
    }
}