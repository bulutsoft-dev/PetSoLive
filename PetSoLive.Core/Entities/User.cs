namespace PetSoLive.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public List<string> Roles { get; set; } = new List<string>();

        // Navigation property to pets owned by the user
        public ICollection<PetOwner> PetOwners { get; set; }  // Collection of pets owned by the user

        // Relationship with Adoption Requests (One User can make many Adoption Requests)
        public ICollection<AdoptionRequest> AdoptionRequests { get; set; } = new List<AdoptionRequest>();
    }
}