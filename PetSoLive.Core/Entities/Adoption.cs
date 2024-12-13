using System;
using PetSoLive.Core.Enums; // Enum için gerekli namespace

namespace PetSoLive.Core.Entities
{
    public class Adoption
    {
        public int Id { get; set; } // Benzersiz kimlik

        public int PetId { get; set; } // İlişkili evcil hayvanın kimliği
        public Pet Pet { get; set; } // Pet ile ilişki

        public int UserId { get; set; } // İlişkili kullanıcının kimliği
        public User User { get; set; } // User ile ilişki

        public DateTime AdoptionDate { get; set; } // Evlat edinme tarihi

        public AdoptionStatus Status { get; set; } // Evlat edinme durumu (Enum)
    }
}