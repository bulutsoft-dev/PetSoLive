namespace Petsolive.API.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? ProfileImageUrl { get; set; }
    public List<string> Roles { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
}