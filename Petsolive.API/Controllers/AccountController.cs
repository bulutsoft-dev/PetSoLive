using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;
using Petsolive.API.Helpers;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;
    private readonly JwtHelper _jwtHelper;

    public AccountController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
        _jwtHelper = new JwtHelper(); // .env'den otomatik okur
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid input data", details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        try
        {
            var user = _mapper.Map<User>(registerDto);
            await _serviceManager.UserService.RegisterAsync(user);
            return Ok(new { message = "User registered successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] AuthDto loginDto)
    {
        var user = await _serviceManager.UserService.AuthenticateAsync(loginDto.Username, loginDto.Password);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var token = _jwtHelper.GenerateToken(user.Id, user.Username, user.Roles);
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            User = userDto
        });
    }
}

