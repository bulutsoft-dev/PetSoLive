namespace Petsolive.API.DTOs;

public class AuthDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public List<string> Roles { get; set; }
    public string Password { get; set; } // <-- Bunu ekle!
}