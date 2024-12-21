namespace PetSoLive.Core.Entities
{
    public class PetOwner
    {
        public int PetId { get; set; }
        public Pet Pet { get; set; }  // Navigation property to Pet

        public int UserId { get; set; }
        public User User { get; set; }  // Navigation property to User

        public DateTime OwnershipDate { get; set; }
    }
    }
