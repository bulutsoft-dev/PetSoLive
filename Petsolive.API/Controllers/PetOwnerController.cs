using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetOwnerController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public PetOwnerController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet("{petId}")]
    public async Task<ActionResult<PetOwnerDto>> GetByPetId(int petId)
    {
        var petOwner = await _serviceManager.PetOwnerService.GetPetOwnerByPetIdAsync(petId);
        if (petOwner == null) return NotFound();
        return Ok(_mapper.Map<PetOwnerDto>(petOwner));
    }
}