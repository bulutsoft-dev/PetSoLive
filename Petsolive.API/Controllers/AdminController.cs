using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public AdminController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet("is-admin/{userId}")]
    public async Task<ActionResult<bool>> IsUserAdmin(int userId)
    {
        var isAdmin = await _serviceManager.AdminService.IsUserAdminAsync(userId);
        return Ok(isAdmin);
    }
}