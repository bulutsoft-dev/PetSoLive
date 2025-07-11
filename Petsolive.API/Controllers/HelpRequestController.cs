using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Petsolive.API.DTOs;
using PetSoLive.Core.Entities;
using PetSoLive.Core;

namespace Petsolive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelpRequestController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;

    public HelpRequestController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HelpRequestDto>>> GetAll()
    {
        var helpRequests = await _serviceManager.HelpRequestService.GetHelpRequestsAsync();
        return Ok(_mapper.Map<IEnumerable<HelpRequestDto>>(helpRequests));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HelpRequestDto>> GetById(int id)
    {
        var helpRequest = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(id);
        if (helpRequest == null) return NotFound();
        return Ok(_mapper.Map<HelpRequestDto>(helpRequest));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] HelpRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        dto.Id = 0; // Id'yi sıfırla, veritabanı otomatik versin
        var entity = _mapper.Map<HelpRequest>(dto);
        // CreatedAt UTC olarak işaretle
        if (entity.CreatedAt.Kind != DateTimeKind.Utc)
            entity.CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
        if (entity.CreatedAt == default)
            entity.CreatedAt = DateTime.UtcNow;
        await _serviceManager.HelpRequestService.CreateHelpRequestAsync(entity);

        // --- EMAIL ---
        // Tüm onaylı veterinerlere e-posta gönder
        var veterinarians = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
        var user = await _serviceManager.UserService.GetUserByIdAsync(entity.UserId);
        var emailHelper = new EmailHelper();
        foreach (var vet in veterinarians)
        {
            if (vet.User != null)
            {
                var body = emailHelper.GenerateVeterinarianNotificationEmailBody(entity, user);
                await _serviceManager.EmailService.SendEmailAsync(vet.User.Email, "New Help Request: Animal in Need!", body);
            }
        }
        // --- EMAIL END ---

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] HelpRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        var entity = _mapper.Map<HelpRequest>(dto);
        entity.Id = id;
        // CreatedAt UTC olarak işaretle
        if (entity.CreatedAt.Kind != DateTimeKind.Utc)
            entity.CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);
        await _serviceManager.HelpRequestService.UpdateHelpRequestAsync(entity);

        // --- EMAIL ---
        // Tüm onaylı veterinerlere güncelleme e-postası gönder
        var veterinarians = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
        var user = await _serviceManager.UserService.GetUserByIdAsync(entity.UserId);
        var emailHelper = new EmailHelper();
        foreach (var vet in veterinarians)
        {
            if (vet.User != null)
            {
                var body = emailHelper.GenerateEditHelpRequestEmailBody(entity, user);
                await _serviceManager.EmailService.SendEmailAsync(vet.User.Email, "Help Request Updated: Animal in Need!", body);
            }
        }
        // --- EMAIL END ---

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        // Silmeden önce ilgili help request'i ve user'ı çek
        var entity = await _serviceManager.HelpRequestService.GetHelpRequestByIdAsync(id);
        var user = entity != null ? await _serviceManager.UserService.GetUserByIdAsync(entity.UserId) : null;
        await _serviceManager.HelpRequestService.DeleteHelpRequestAsync(id);

        // --- EMAIL ---
        // Tüm onaylı veterinerlere silinme e-postası gönder
        if (entity != null && user != null)
        {
            var veterinarians = await _serviceManager.VeterinarianService.GetAllVeterinariansAsync();
            var emailHelper = new EmailHelper();
            foreach (var vet in veterinarians)
            {
                if (vet.User != null)
                {
                    var body = emailHelper.GenerateDeleteHelpRequestEmailBody(entity, user);
                    await _serviceManager.EmailService.SendEmailAsync(vet.User.Email, "Help Request Deleted: Animal in Need!", body);
                }
            }
        }
        // --- EMAIL END ---

        return NoContent();
    }
}