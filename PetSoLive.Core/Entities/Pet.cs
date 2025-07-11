namespace PetSoLive.Core.Entities
{
    public class Pet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public double Weight { get; set; }
        public string Color { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Description { get; set; }
        public string VaccinationStatus { get; set; }
        public string MicrochipId { get; set; }
        public bool? IsNeutered { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<AdoptionRequest> AdoptionRequests { get; set; } = new List<AdoptionRequest>();
        public ICollection<PetOwner> PetOwners { get; set; } = new List<PetOwner>();
    }
}