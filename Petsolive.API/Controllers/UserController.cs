using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _serviceManager.UserService.GetUsersByLocationAsync("", "");
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _serviceManager.UserService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto userDto)
    {
        var user = _mapper.Map<PetSoLive.Core.Entities.User>(userDto);
        user.Id = id;
        await _serviceManager.UserService.UpdateUserAsync(user);
        return NoContent();
    }
}