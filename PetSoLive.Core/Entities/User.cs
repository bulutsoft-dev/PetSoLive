namespace PetSoLive.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        // This list holds the roles assigned to the user, used for authorization.
        // It is initialized as a new list of strings, meaning it starts empty but ready to be added to.
        public List<string> Roles { get; set; } = new List<string>();
    }
}

