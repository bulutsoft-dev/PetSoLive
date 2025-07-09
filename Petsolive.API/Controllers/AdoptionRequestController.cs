using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdoptionRequestController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public AdoptionRequestController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdoptionRequestDto>> GetById(int id)
    {
        var req = await _serviceManager.AdoptionRequestService.GetAdoptionRequestByIdAsync(id);
        if (req == null) return NotFound();
        return Ok(_mapper.Map<AdoptionRequestDto>(req));
    }

    [HttpGet("pet/{petId}")]
    public async Task<ActionResult<IEnumerable<AdoptionRequestDto>>> GetByPetId(int petId)
    {
        var requests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(petId);
        if (requests == null || !requests.Any())
            return NotFound();

        var dtos = requests.Select(r => _mapper.Map<AdoptionRequestDto>(r));
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AdoptionRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            var entity = _mapper.Map<PetSoLive.Core.Entities.AdoptionRequest>(dto);
            entity.Id = 0; // Yeni kayıt için
            entity.Status = PetSoLive.Core.Enums.AdoptionStatus.Pending;
            entity.RequestDate = DateTime.UtcNow;

            await _serviceManager.AdoptionService.CreateAdoptionRequestAsync(entity);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}