namespace PetSoLive.API.DTOs
{
    public class LostPetAdDto
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public string Description { get; set; }
        public DateTime LastSeenDate { get; set; }
        public string ImageUrl { get; set; }
        public int UserId { get; set; }

        public string LastSeenCity { get; set; }
        public string LastSeenDistrict { get; set; }
        public DateTime CreatedAt { get; set; }

        // Opsiyonel: Görsel amaçlı birleştirilmiş alan
        public string LastSeenLocation => $"{LastSeenCity}, {LastSeenDistrict}";
        
        // Eğer User bilgisi gerekliyse sadece basit kullanıcı adı gibi bilgiler DTO olarak eklenebilir:
        public string UserName { get; set; }    // User entity'den alınarak Map edilebilir
    }
}