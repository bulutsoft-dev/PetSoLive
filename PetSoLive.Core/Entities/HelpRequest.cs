using System.ComponentModel.DataAnnotations;
using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Entities
{
    public class HelpRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title can't be longer than 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Emergency Level is required.")]
        public EmergencyLevel EmergencyLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; } // Navigation property

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location can't be longer than 200 characters.")]
        public string Location { get; set; }

        [StringLength(100, ErrorMessage = "Contact Name can't be longer than 100 characters.")]
        public string? ContactName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? ContactPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? ContactEmail { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public HelpRequestStatus Status { get; set; }  // Enum field for status

        // Comments associated with the HelpRequest (One-to-many relationship)
        public virtual ICollection<Comment>? Comments { get; set; } // Navigation property for comments
    }
}