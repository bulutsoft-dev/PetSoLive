using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdoptionController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public AdoptionController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet("{petId}")]
    public async Task<ActionResult<IEnumerable<AdoptionDto>>> GetByPetId(int petId, [FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        if (page.HasValue && pageSize.HasValue)
        {
            var adoptions = await _serviceManager.AdoptionService.GetAdoptionsPagedAsync(page.Value, pageSize.Value);
            var adoptionDtos = _mapper.Map<IEnumerable<AdoptionDto>>(adoptions);
            return Ok(adoptionDtos);
        }
        else
        {
            var adoption = await _serviceManager.AdoptionService.GetAdoptionByPetIdAsync(petId);
            if (adoption == null) return NotFound();
            return Ok(_mapper.Map<AdoptionDto>(adoption));
        }
    }

    [HttpGet("paged")]
    public async Task<ActionResult<IEnumerable<AdoptionDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var adoptions = await _serviceManager.AdoptionService.GetAdoptionsPagedAsync(page, pageSize);
        var adoptionDtos = _mapper.Map<IEnumerable<AdoptionDto>>(adoptions);
        return Ok(adoptionDtos);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AdoptionDto adoptionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var adoption = _mapper.Map<Adoption>(adoptionDto);
        // AdoptionDate UTC olarak işaretle
        if (adoption.AdoptionDate.Kind != DateTimeKind.Utc)
            adoption.AdoptionDate = DateTime.SpecifyKind(adoption.AdoptionDate, DateTimeKind.Utc);
        if (adoption.AdoptionDate == default)
            adoption.AdoptionDate = DateTime.UtcNow;
        await _serviceManager.AdoptionService.CreateAdoptionAsync(adoption);
        return Ok();
    }
}