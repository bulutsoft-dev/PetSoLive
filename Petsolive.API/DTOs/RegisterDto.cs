using System.ComponentModel.DataAnnotations;

namespace Petsolive.API.DTOs;

public class RegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    public string? ProfileImageUrl { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
} 