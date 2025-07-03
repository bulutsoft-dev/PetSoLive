using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Data;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public AdminController(IServiceManager serviceManager, IMapper mapper, ApplicationDbContext context)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
        _context = context;
    }

    [HttpGet("is-admin/{userId}")]
    public async Task<ActionResult<bool>> IsUserAdmin(int userId)
    {
        var isAdmin = await _serviceManager.AdminService.IsUserAdminAsync(userId);
        return Ok(isAdmin);
    }

    /// <summary>
    /// Adoptions tablosunun Id sequence'ini sıfırlar. Sadece development/test için kullanın!
    /// </summary>
    [HttpPost("fix-adoption-sequence")]
    public async Task<IActionResult> FixAdoptionIdSequence()
    {
        var maxId = await _context.Adoptions.MaxAsync(a => (int?)a.Id) ?? 0;
        var sql = $"SELECT setval('\"Adoptions_Id_seq\"', {maxId})";
        await _context.Database.ExecuteSqlRawAsync(sql);
        return Ok(new { message = "Adoptions Id sequence sıfırlandı.", maxId });
    }
}