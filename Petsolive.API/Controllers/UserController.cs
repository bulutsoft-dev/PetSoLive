using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Petsolive.API.DTOs;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public UserController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    /// <summary>Tüm kullanıcıları getirir.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _serviceManager.UserService.GetAllUsersAsync();
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return Ok(userDtos);
    }

    /// <summary>Belirli kullanıcıyı getirir.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _serviceManager.UserService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(_mapper.Map<UserDto>(user));
    }

    /// <summary>Kullanıcıyı günceller.</summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto userDto)
    {
        var user = _mapper.Map<PetSoLive.Core.Entities.User>(userDto);
        user.Id = id;
        // DateTime alanlarını UTC olarak işaretle
        if (user.DateOfBirth.Kind != DateTimeKind.Utc)
            user.DateOfBirth = DateTime.SpecifyKind(user.DateOfBirth, DateTimeKind.Utc);
        if (user.CreatedDate.Kind != DateTimeKind.Utc)
            user.CreatedDate = DateTime.SpecifyKind(user.CreatedDate, DateTimeKind.Utc);
        if (user.LastLoginDate.HasValue && user.LastLoginDate.Value.Kind != DateTimeKind.Utc)
            user.LastLoginDate = DateTime.SpecifyKind(user.LastLoginDate.Value, DateTimeKind.Utc);
        await _serviceManager.UserService.UpdateUserAsync(user);
        return NoContent();
    }
}