namespace PetSoLive.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; } // Kullanıcının telefon numarası
        public string Address { get; set; } // Kullanıcının adres bilgisi
        public DateTime DateOfBirth { get; set; } // Kullanıcının doğum tarihi
        public bool IsActive { get; set; } // Kullanıcı hesabı aktif mi?
        public DateTime CreatedDate { get; set; } // Hesap oluşturulma tarihi
        public DateTime? LastLoginDate { get; set; } // Son giriş tarihi
        public string ProfileImageUrl { get; set; } // Profil resmi URL'si

        public List<string> Roles { get; set; } = new List<string>();

        // Navigation property to pets owned by the user
        public ICollection<PetOwner> PetOwners { get; set; } = new List<PetOwner>();  // Kullanıcının sahip olduğu evcil hayvanlar

        // Relationship with Adoption Requests (One User can make many Adoption Requests)
        public ICollection<AdoptionRequest> AdoptionRequests { get; set; } = new List<AdoptionRequest>();

    }
    }
