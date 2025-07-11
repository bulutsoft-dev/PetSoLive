using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using PetSoLive.API.DTOs;
using PetSoLive.Core.Entities;
using PetSoLive.Core;

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
    [Authorize]
    public async Task<IActionResult> Create([FromBody] LostPetAdDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        dto.Id = 0; // Id'yi sıfırla, veritabanı otomatik versin
        var entity = _mapper.Map<LostPetAd>(dto);
        // LastSeenDate ve CreatedAt UTC olarak işaretle
        if (entity.LastSeenDate.Kind != DateTimeKind.Utc)
            entity.LastSeenDate = DateTime.SpecifyKind(entity.LastSeenDate, DateTimeKind.Utc);
        if (entity.LastSeenDate == default)
            entity.LastSeenDate = DateTime.UtcNow;
        if (entity.CreatedAt.Kind != DateTimeKind.Utc)
            entity.CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
        if (entity.CreatedAt == default)
            entity.CreatedAt = DateTime.UtcNow;
        await _serviceManager.LostPetAdService.CreateLostPetAdAsync(entity, dto.LastSeenCity, dto.LastSeenDistrict);

        // --- EMAIL ---
        var user = await _serviceManager.UserService.GetUserByIdAsync(entity.UserId);
        var emailHelper = new EmailHelper();
        if (user != null)
        {
            var body = emailHelper.GenerateNewLostPetAdEmailBody(entity, user);
            await _serviceManager.EmailService.SendEmailAsync(user.Email, "Lost Pet Ad Created", body);
        }
        // --- EMAIL END ---

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] LostPetAdDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var entity = _mapper.Map<LostPetAd>(dto);
        entity.Id = id;
        // LastSeenDate ve CreatedAt UTC olarak işaretle
        if (entity.LastSeenDate.Kind != DateTimeKind.Utc)
            entity.LastSeenDate = DateTime.SpecifyKind(entity.LastSeenDate, DateTimeKind.Utc);
        if (entity.CreatedAt.Kind != DateTimeKind.Utc)
            entity.CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
        await _serviceManager.LostPetAdService.UpdateLostPetAdAsync(entity);

        // --- EMAIL ---
        var user = await _serviceManager.UserService.GetUserByIdAsync(entity.UserId);
        var emailHelper = new EmailHelper();
        if (user != null)
        {
            var body = emailHelper.GenerateUpdatedLostPetAdEmailBody(entity, user);
            await _serviceManager.EmailService.SendEmailAsync(user.Email, "Lost Pet Ad Updated", body);
        }
        // --- EMAIL END ---

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var ad = await _serviceManager.LostPetAdService.GetLostPetAdByIdAsync(id);
        if (ad == null) return NotFound();
        var user = ad.UserId != 0 ? await _serviceManager.UserService.GetUserByIdAsync(ad.UserId) : null;
        await _serviceManager.LostPetAdService.DeleteLostPetAdAsync(ad);

        // --- EMAIL ---
        var emailHelper = new EmailHelper();
        if (user != null)
        {
            var body = emailHelper.GenerateDeletedLostPetAdEmailBody(ad, user);
            await _serviceManager.EmailService.SendEmailAsync(user.Email, "Lost Pet Ad Deleted", body);
        }
        // --- EMAIL END ---

        return NoContent();
    }
}