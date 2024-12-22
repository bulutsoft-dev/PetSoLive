namespace PetSoLive.Core.Entities
{
    public class PetOwner
    {
        public int PetId { get; set; }
        public Pet Pet { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime OwnershipDate { get; set; }
    }
}