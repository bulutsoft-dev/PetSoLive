using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Petsolive.API.DTOs;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeterinarianController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public VeterinarianController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeterinarianDto>>> GetAll()
    {
        var vets = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
        return Ok(_mapper.Map<IEnumerable<VeterinarianDto>>(vets));
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<ActionResult<VeterinarianDto>> Register([FromBody] VeterinarianDto vetDto)
    {
        var vet = await _serviceManager.VeterinarianService.RegisterVeterinarianAsync(
            vetDto.UserId, vetDto.Qualifications, vetDto.ClinicAddress, vetDto.ClinicPhoneNumber);
        // AppliedDate UTC olarak işaretle
        if (vet.AppliedDate.Kind != DateTimeKind.Utc)
            vet.AppliedDate = DateTime.SpecifyKind(vet.AppliedDate, DateTimeKind.Utc);
        if (vet.AppliedDate == default)
            vet.AppliedDate = DateTime.UtcNow;
        return Ok(_mapper.Map<VeterinarianDto>(vet));
    }

    [HttpPut("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(int id)
    {
        await _serviceManager.VeterinarianService.ApproveVeterinarianAsync(id);
        return NoContent();
    }

    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(int id)
    {
        await _serviceManager.VeterinarianService.RejectVeterinarianAsync(id);
        return NoContent();
    }
}