namespace Petsolive.API.DTOs;

// AuthResponseDto.cs
public class AuthResponseDto
{
    public string Token { get; set; }
    public UserDto User { get; set; }
}