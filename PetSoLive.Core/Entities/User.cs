// 2. Core Layer (/PetSoLive.Core)

// Entities (Domain models)
namespace PetSoLive.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
        
}