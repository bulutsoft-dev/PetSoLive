namespace PetSoLive.Core.Entities
{
    public class LostPetAd
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public string Description { get; set; }
        public DateTime LastSeenDate { get; set; }
        public string ImageUrl { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string LastSeenCity { get; set; }
        public string LastSeenDistrict { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string LastSeenLocation => $"{LastSeenCity}, {LastSeenDistrict}";
    }
}