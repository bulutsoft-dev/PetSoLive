using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
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
    public async Task<ActionResult<AdoptionDto>> GetByPetId(int petId)
    {
        var adoption = await _serviceManager.AdoptionService.GetAdoptionByPetIdAsync(petId);
        if (adoption == null) return NotFound();
        return Ok(_mapper.Map<AdoptionDto>(adoption));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdoptionDto adoptionDto)
    {
        var adoption = _mapper.Map<Adoption>(adoptionDto);
        await _serviceManager.AdoptionService.CreateAdoptionAsync(adoption);
        return Ok();
    }
}