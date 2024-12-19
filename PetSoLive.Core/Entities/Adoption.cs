using System;
using PetSoLive.Core.Enums; // Enum için gerekli namespace

using System;
using PetSoLive.Core.Enums; // Enum için gerekli namespace

namespace PetSoLive.Core.Entities
{
    public class Adoption
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public int UserId { get; set; }
        public DateTime AdoptionDate { get; set; }
        public AdoptionStatus Status { get; set; }

        // Navigation Properties
        public Pet Pet { get; set; }
        public User User { get; set; }
    }
}
