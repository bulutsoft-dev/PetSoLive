using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using PetSoLive.API.DTOs;
using PetSoLive.Core.Entities;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LostPetAdController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public LostPetAdController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LostPetAdDto>>> GetAll()
    {
        var ads = await _serviceManager.LostPetAdService.GetAllLostPetAdsAsync();
        return Ok(_mapper.Map<IEnumerable<LostPetAdDto>>(ads));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LostPetAdDto>> GetById(int id)
    {
        var ad = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);
        if (ad == null) return NotFound();
        return Ok(_mapper.Map<LostPetAdDto>(ad));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LostPetAdDto dto)
    {
        dto.Id = 0; // Id'yi sıfırla, veritabanı otomatik versin
        var entity = _mapper.Map<LostPetAd>(dto);
        await _serviceManager.LostPetAdService.CreateLostPetAdAsync(entity, dto.LastSeenCity, dto.LastSeenDistrict);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LostPetAdDto dto)
    {
        var entity = _mapper.Map<LostPetAd>(dto);
        entity.Id = id;
        await _serviceManager.LostPetAdService.UpdateLostPetAdAsync(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ad = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);
        if (ad == null) return NotFound();
        await _serviceManager.LostPetAdService.DeleteLostPetAdAsync(ad);
        return NoContent();
    }
}