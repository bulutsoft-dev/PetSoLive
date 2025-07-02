namespace Petsolive.API.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public int HelpRequestId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int? VeterinarianId { get; set; }
    public string? VeterinarianName { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}