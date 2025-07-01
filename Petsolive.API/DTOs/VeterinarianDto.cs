namespace Petsolive.API.DTOs;

public class VeterinarianDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Qualifications { get; set; }
    public string ClinicAddress { get; set; }
    public string ClinicPhoneNumber { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } // Enum string olarak
}