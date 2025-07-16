namespace PetSoLive.Core.DTOs
{
    public class PetFilterDto
    {
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public string? Gender { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? Color { get; set; }
        public bool? IsAdopted { get; set; }
    }
} 