namespace Petsolive.API.DTOs;

public class HelpRequestDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string EmergencyLevel { get; set; } // Enum string olarak
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Location { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } // Enum string olarak
}