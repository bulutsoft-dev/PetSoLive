namespace PetSoLive.Core.Entities
{
    public class Admin
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // User ile ilişkilendirme
        public DateTime CreatedDate { get; set; }  // Admin olarak kaydedildiği tarih

        public User User { get; set; }  // Admin ile ilişkilendirilmiş kullanıcı
    }
}