using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public PetController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetDto>>> GetAll()
    {
        var pets = await _serviceManager.PetService.GetAllPetsAsync();
        var petDtos = _mapper.Map<IEnumerable<PetDto>>(pets);
        return Ok(petDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PetDto>> GetById(int id)
    {
        var pet = await _serviceManager.PetService.GetPetByIdAsync(id);
        if (pet == null) return NotFound();
        return Ok(_mapper.Map<PetDto>(pet));
    }

    [HttpPost]
    public async Task<ActionResult<PetDto>> Create([FromBody] PetDto petDto)
    {
        var pet = _mapper.Map<Pet>(petDto);
        await _serviceManager.PetService.CreatePetAsync(pet);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, _mapper.Map<PetDto>(pet));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PetDto petDto)
    {
        var pet = _mapper.Map<Pet>(petDto);
        await _serviceManager.PetService.UpdatePetAsync(id, pet, 0); // userId parametresi örnek
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _serviceManager.PetService.DeletePetAsync(id, 0); // userId parametresi örnek
        return NoContent();
    }
}