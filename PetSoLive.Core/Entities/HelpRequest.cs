using PetSoLive.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace PetSoLive.Core.Entities
{
    public class HelpRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title can't be longer than 100 characters.")]
        public string Title { get; set; }  // New Title field

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Emergency Level is required.")]
        public EmergencyLevel EmergencyLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }  // Foreign Key

        public User? User { get; set; }  // Nullable navigation property for User

        // Yeni Alanlar:
        
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location can't be longer than 200 characters.")]
        public string Location { get; set; }  // Bulunduğu yer bilgisi

        [StringLength(100, ErrorMessage = "Contact Name can't be longer than 100 characters.")]
        public string? ContactName { get; set; }  // İlgili kişinin adı (isteğe bağlı)

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? ContactPhone { get; set; }  // İlgili kişinin telefon numarası (isteğe bağlı)

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? ContactEmail { get; set; }  // İlgili kişinin e-posta adresi (isteğe bağlı)

        public string? ImageUrl { get; set; }  // Hayvanın resmi için URL
    }
}