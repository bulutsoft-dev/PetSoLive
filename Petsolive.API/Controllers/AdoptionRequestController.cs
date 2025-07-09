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

    [HttpPut("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(int id)
    {
        using var transaction = await HttpContext.RequestServices.GetService<Microsoft.EntityFrameworkCore.DbContext>()?.Database.BeginTransactionAsync();
        try
        {
            var request = await _serviceManager.AdoptionRequestService.GetAdoptionRequestByIdAsync(id);
            if (request == null)
                return NotFound();

            if (request.Status == PetSoLive.Core.Enums.AdoptionStatus.Approved)
                return BadRequest(new { error = "Request is already approved." });

            request.Status = PetSoLive.Core.Enums.AdoptionStatus.Approved;
            await _serviceManager.AdoptionRequestService.UpdateAdoptionRequestAsync(request);

            // Diğer tüm başvuruları reddet
            var allRequests = await _serviceManager.AdoptionRequestService.GetAdoptionRequestsByPetIdAsync(request.PetId);
            foreach (var r in allRequests)
            {
                if (r.Id != id && r.Status == PetSoLive.Core.Enums.AdoptionStatus.Pending)
                {
                    r.Status = PetSoLive.Core.Enums.AdoptionStatus.Rejected;
                    await _serviceManager.AdoptionRequestService.UpdateAdoptionRequestAsync(r);
                    // İsteğe bağlı: Red maili gönder
                    // await _serviceManager.EmailService.SendEmailAsync(r.User.Email, "Adoption Request Rejected", "Your request was rejected.");
                }
            }

            // Adoption tablosuna kayıt ekle
            var adoption = new PetSoLive.Core.Entities.Adoption
            {
                PetId = request.PetId,
                UserId = request.UserId,
                AdoptionDate = DateTime.UtcNow,
                Status = PetSoLive.Core.Enums.AdoptionStatus.Approved
            };
            await _serviceManager.AdoptionService.CreateAdoptionAsync(adoption);

            // Onay maili gönder
            // await _serviceManager.EmailService.SendEmailAsync(request.User.Email, "Adoption Request Approved", "Your request was approved!");

            if (transaction != null)
                await transaction.CommitAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            if (transaction != null)
                await transaction.RollbackAsync();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            var request = await _serviceManager.AdoptionRequestService.GetAdoptionRequestByIdAsync(id);
            if (request == null)
                return NotFound();

            if (request.Status == PetSoLive.Core.Enums.AdoptionStatus.Rejected)
                return BadRequest(new { error = "Request is already rejected." });

            request.Status = PetSoLive.Core.Enums.AdoptionStatus.Rejected;
            await _serviceManager.AdoptionRequestService.UpdateAdoptionRequestAsync(request);

            // Red maili gönder
            // await _serviceManager.EmailService.SendEmailAsync(request.User.Email, "Adoption Request Rejected", "Your request was rejected.");

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}